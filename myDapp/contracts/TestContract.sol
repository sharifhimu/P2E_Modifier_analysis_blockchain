// contracts/TestContract.sol
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

contract TestContract {
    uint public ethPrice;

    struct Score {
        uint playerId;
        string walletAddress;
        uint coinAmount;
        uint characterIndex;
        uint exchangedAmount;
        bool exists;
    }

    uint public minClamp;
    uint public midClamp;
    uint public maxClamp;

    constructor() {
        minClamp = 30; 
        midClamp = 65; 
        maxClamp = 110;
    }

    mapping(address => Score) public playerScores;
    address[] public playerAddresses;

    function hello() public pure returns (string memory) {
        return "Hello World";
    }

    function setPrice(uint _price) public {
        ethPrice = _price;
    }

    event ScoreSubmitted(address sender, uint playerId, string wallet, uint coin, uint character);

    function Sendscore(uint playerId, string memory add, uint coinAmount, uint characterIndex) public {
        
            if( !playerScores[msg.sender].exists ) {
                playerAddresses.push(msg.sender);
            }

            Score memory newScore = Score(playerId, add, coinAmount, characterIndex, 0, true);
            playerScores[msg.sender] = newScore;

            emit ScoreSubmitted(msg.sender, playerId, add, coinAmount, characterIndex);
    }

    function updateScoreByPlayerId(
        uint _newCoinAmount,
        uint _newCharacterIndex,
        uint _exchangedAmount
    ) public {
        // Access the player's score
        Score storage score = playerScores[msg.sender];

        // Optional: require the record to exist — this assumes the 'exists' flag
        require(score.exists, "No score found for this address" );

        // Update values
        score.coinAmount = _newCoinAmount;
        score.characterIndex = _newCharacterIndex;

        // Only update if _exchangedAmount is not sentinel
        if (_exchangedAmount != type(uint).max) {
            score.exchangedAmount = _exchangedAmount;
        }
        
    }

    function setExchangeAmount(uint _amount) public {
        Score storage score = playerScores[msg.sender];
        // require(score.exists, "Player does not exist");
        score.exchangedAmount = _amount;
    }

    function getAllData() public view returns (
        uint,
        Score[] memory
    ) {
        Score[] memory scores = new Score[](playerAddresses.length);
        for (uint i = 0; i < playerAddresses.length; i++) {
            scores[i] = playerScores[playerAddresses[i]];
        }
        return (ethPrice, scores);
    }

    function getPlayerData(address _wallet) public view returns (
        uint playerId,
        string memory walletAddress,
        uint coinAmount,
        uint characterIndex,
        uint exchangedAmount,
        bool exists
    ) {
        Score memory player = playerScores[_wallet];
        return (
            player.playerId,
            player.walletAddress,
            player.coinAmount,
            player.characterIndex,
            player.exchangedAmount,
            player.exists
        );
    }

    function getClamps() public view returns (uint256, uint256, uint256) {
        return (minClamp, midClamp, maxClamp);
    }

    function setModifiers(uint _min, uint _mid, uint _max) public {
        minClamp = _min*100;
        midClamp = _mid*100;
        maxClamp = _max*100;
    }

}