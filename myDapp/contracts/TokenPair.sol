// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/cryptography/MerkleProof.sol";

// LP Token Contract
contract LPToken is ERC20 {
    address public pool;

    constructor() ERC20("Liquidity Pool Token", "LPT") {
        pool = msg.sender; // TokenPair will be owner
    }

    function mint(address to, uint256 amount) external {
        require(msg.sender == pool, "Only pool can mint");
        _mint(to, amount);
    }

    function burn(address from, uint256 amount) external {
        require(msg.sender == pool, "Only pool can burn");
        _burn(from, amount);
    }
}

contract TokenPair is Ownable {
    // ===== your existing state =====
    address public token0;
    address public token1;

    uint112 private reserve0; // x
    uint112 private reserve1; // y

    LPToken public lpToken;

    uint16 public impactToleranceBps = 100; // default 1% (100 bps = 0.01)

    enum ExtraLogicType { BalanceBoost, FlatBoost }

    ExtraLogicType public extraLogic = ExtraLogicType.BalanceBoost; // default
    uint256 public balanceBoostBps = 250; // 2.5% (bps = basis points, 250 = 2.5%)
    uint256 public flatBoostAmount = 100 ether; // Example: flat +100 tokens

    // ============================================================
    // =============== Epoch + Merkle Allowances =============
    // ============================================================
    // per-epoch Merkle roots (epochId => root)
    mapping(uint256 => bytes32) public merkleRootOf;

    // per-epoch per-user used amount (to enforce allowance across many swaps)
    mapping(uint256 => mapping(address => uint256)) public usedBy;

    // epoch duration (DAO adjustable)
    uint256 public epochDuration;

    constructor(address _token0, address _token1) {
        token0 = _token0;
        token1 = _token1;
        lpToken = new LPToken();

        // initialize epoch duration default (15 minutes for testing)
        epochDuration = 5 minutes;
    }

    // debug events
    event LogRootSet(uint256 indexed epochId, bytes32 root);
    event LogLeafComputed(uint256 indexed epochId, address indexed who, uint256 allowance, bytes32 leaf);
    event DebugEpoch(uint256 currentEpoch, bytes32 storedRoot);

    event ExtraLogicChanged(ExtraLogicType newLogic);
    event BalanceBoostBpsChanged(uint256 newBps);
    event FlatBoostChanged(uint256 newFlat);



    // DAO can switch the logic type
    function setExtraLogic(ExtraLogicType newLogic) external onlyOwner {
        extraLogic = newLogic;
        emit ExtraLogicChanged(newLogic);
    }

    // DAO can update the balance-boost percentage (e.g. from 2.5% to 5%)
    function setBalanceBoostBps(uint256 newBps) external onlyOwner {
        require(newBps <= 5000, "too high"); // max 50%
        balanceBoostBps = newBps;
        emit BalanceBoostBpsChanged(newBps);
    }

    // DAO can update the flat boost amount
    function setFlatBoostAmount(uint256 newFlat) external onlyOwner {
        flatBoostAmount = newFlat;
        emit FlatBoostChanged(newFlat);
    }

    // ---- Allowance calculation ----
    function computeAllowance(address player) public view returns (uint256) {
        uint256 base = _computeMaxSwapInForImpact(true); // safe cap for token0->token1
        uint256 extra = 0;

        if (extraLogic == ExtraLogicType.BalanceBoost) {
            uint256 bal = IERC20(token0).balanceOf(player);
            extra = (bal * balanceBoostBps) / 10_000; // % of balance
        } else if (extraLogic == ExtraLogicType.FlatBoost) {
            extra = flatBoostAmount; // fixed amount
        }

        uint256 total = base + extra;
        uint256 playerBal = IERC20(token0).balanceOf(player);

        // Cap the allowance at the player's balance
        if (total > playerBal) {
            return playerBal;
        }
        return total;
    }

    // helper to compute leaf on-chain (pure) for comparison
    function computeLeaf(uint256 epochId, address player, uint256 allowance) public pure returns (bytes32) {
        return keccak256(abi.encodePacked(epochId, player, allowance));
    }

    function getReserves() public view returns (uint112, uint112) {
        return (reserve0, reserve1);
    }

    // ===== Governance-controlled setter (via voting) =====
    function setImpactToleranceBps(uint16 newBps) external onlyOwner {
        require(newBps > 0 && newBps < 10_000, "bad bps");
        impactToleranceBps = newBps;
    }

    // ===== View: Max safe swap input (external wrapper) =====
    // - Only applies to token0->token1 swaps
    // - If isToken0To1 = false, returns MAX (no limit, game profit)
    function getMaxSwapInForImpact(bool isToken0To1) external view returns (uint256 maxIn) {
        return _computeMaxSwapInForImpact(isToken0To1);
    }

    // Internal helper moved here to avoid "stack too deep" in swapWithProof.
    function _computeMaxSwapInForImpact(bool isToken0To1) internal view returns (uint256 maxIn) {
        (uint112 x, uint112 y) = getReserves();

        if (!isToken0To1) {
            return type(uint256).max; // unlimited for token1->token0
        }

        uint16 maxImpactBps = impactToleranceBps;

        // ==== Step 1: Compute safe output cap (reserve1 side) ====
        uint256 maxReserve1Out = (uint256(y) * maxImpactBps) / 10_000;

        // ==== Step 2: Back-compute required input token0 ====
        // Δx = (x * Δy) / (y - Δy)
        if (maxReserve1Out >= y) {
            return 0; // can't drain entire pool
        }

        uint256 numerator = uint256(x) * maxReserve1Out;
        uint256 denominator = uint256(y) - maxReserve1Out;
        uint256 requiredIn = numerator / denominator;

        maxIn = requiredIn;

        // Extra guard: never allow more than 20% of reserve0
        uint256 twentyPercent = uint256(x) / 5;
        if (maxIn > twentyPercent) maxIn = twentyPercent;
    }

    // Add liquidity (LP tokens minted proportional to contribution)
    function addLiquidity(uint112 amount0, uint112 amount1) public {
        IERC20(token0).transferFrom(msg.sender, address(this), amount0);
        IERC20(token1).transferFrom(msg.sender, address(this), amount1);

        uint256 liquidity;
        if (reserve0 == 0 && reserve1 == 0) {
            // First deposit: simple sum
            liquidity = sqrt(uint256(amount0) * uint256(amount1));
        } else {
            // Proportional LP minting
            liquidity = min(
                (uint256(amount0) * lpToken.totalSupply()) / reserve0,
                (uint256(amount1) * lpToken.totalSupply()) / reserve1
            );
        }

        require(liquidity > 0, "Insufficient liquidity minted");

        reserve0 += amount0;
        reserve1 += amount1;

        lpToken.mint(msg.sender, liquidity);
    }

    // Swap using constant product formula (x * y = k)
    function swap(uint amountIn, bool isToken0To1) internal {
        if (isToken0To1) {
            IERC20(token0).transferFrom(msg.sender, address(this), amountIn);
            uint amountOut = getAmountOut(amountIn, reserve0, reserve1);
            IERC20(token1).transfer(msg.sender, amountOut);
            reserve0 += uint112(amountIn);
            reserve1 -= uint112(amountOut);
        } else {
            IERC20(token1).transferFrom(msg.sender, address(this), amountIn);
            uint amountOut = getAmountOut(amountIn, reserve1, reserve0);
            IERC20(token0).transfer(msg.sender, amountOut);
            reserve1 += uint112(amountIn);
            reserve0 -= uint112(amountOut);
        }
    }

    function getAmountOut(uint amountIn, uint reserveIn, uint reserveOut) internal pure returns (uint) {
        require(amountIn > 0, "Insufficient input amount");
        require(reserveIn > 0 && reserveOut > 0, "Insufficient liquidity");

        uint amountInWithFee = amountIn * 997; // 0.3% fee
        uint numerator = amountInWithFee * reserveOut;
        uint denominator = (reserveIn * 1000) + amountInWithFee;
        return numerator / denominator;
    }

    function sqrt(uint y) internal pure returns (uint z) {
        if (y > 3) {
            z = y;
            uint x = y / 2 + 1;
            while (x < z) {
                z = x;
                x = (y / x + x) / 2;
            }
        } else if (y != 0) {
            z = 1;
        }
    }

    function min(uint x, uint y) internal pure returns (uint) {
        return x < y ? x : y;
    }

    // --- Epoch helpers ---
    function setEpochDuration(uint256 newDuration) external onlyOwner {
        require(newDuration >= 60, "epoch too short");
        epochDuration = newDuration;
    }

    function currentEpochId() public view returns (uint256) {
        require(epochDuration > 0, "epoch not set");
        return block.timestamp / epochDuration;
    }

    /// @notice DAO sets the Merkle root for a specific epoch (usually the current one).
    function setMerkleRoot(uint256 epochId, bytes32 root) external onlyOwner {
        merkleRootOf[epochId] = root;
        emit LogRootSet(epochId, root);
    }

    function checkProof(uint256 epochId, address sender, uint256 allowance, bytes32[] calldata proof) 
    external view 
    returns (bool, uint256, bytes32, bool) {
        bytes32 root = merkleRootOf[epochId];
        bytes32 leaf = keccak256(abi.encodePacked(epochId, sender, allowance));
        bool proofIsValid = MerkleProof.verify(proof, root, leaf);
        uint256 currentEpoch = currentEpochId();

        return ( proofIsValid, currentEpoch, root, root != bytes32(0) );
    }

    /// @notice Swap that checks Merkle allowance for the caller in the *current epoch*.
    /// Leaf schema: keccak256(abi.encodePacked(epochId, player, allowance))
    function swapWithProof(
        uint256 amountIn,
        bool isToken0To1,
        uint256 allowance,
        bytes32[] calldata proof,
        uint256 epochId
    ) external {

        // require a root for this epoch
        bytes32 root = merkleRootOf[epochId];
        emit DebugEpoch(epochId, root);   // <--- LOG EPOCH & ROOT
        require(root != bytes32(0), "root not set");

        // verify leaf
        bytes32 leaf = keccak256(abi.encodePacked(epochId, msg.sender, allowance));
        emit LogLeafComputed(epochId, msg.sender, allowance, leaf);
        require(MerkleProof.verify(proof, root, leaf), "bad proof");

        // enforce per-player allowance
        uint256 already = usedBy[epochId][msg.sender];
        require(already + amountIn <= allowance, "allowance exceeded");
        usedBy[epochId][msg.sender] = already + amountIn;

        // optional: also enforce per-tx safe cap for token0->token1 using current reserves
        if (isToken0To1) {
            // uint256 maxIn = _computeMaxSwapInForImpact(true);
            uint256 maxIn = computeAllowance(msg.sender);
            require(amountIn <= maxIn, "above safe cap");
        }

        // execute the existing swap logic
        swap(amountIn, isToken0To1);
    }

}
