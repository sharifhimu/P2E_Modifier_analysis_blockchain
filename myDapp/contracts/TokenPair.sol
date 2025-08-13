// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";

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

contract TokenPair {
    address public token0;
    address public token1;

    uint112 private reserve0; // x
    uint112 private reserve1; // y

    LPToken public lpToken;

    constructor(address _token0, address _token1) {
        token0 = _token0;
        token1 = _token1;
        lpToken = new LPToken();
    }

    function getReserves() public view returns (uint112, uint112) {
        return (reserve0, reserve1);
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
    function swap(uint amountIn, bool isToken0To1) public {
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
}
