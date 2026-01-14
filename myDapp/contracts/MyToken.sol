// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

// const TokenPair = artifacts.require("TokenPair")
// let c = await TokenPair.deployed()          // or TokenPair.at("0xYourAddress")
// let accounts = await web3.eth.getAccounts()
// let owner = accounts[0]                      // adjust if owner is different
// let amount = web3.utils.toWei("10", "ether")
// await c.mintRewards(to, amount, { from: owner })

contract MyToken is ERC20, Ownable {
    constructor(uint256 initialSupply) ERC20("MyToken", "MYT") {
        _mint(msg.sender, initialSupply);
    }

    // For simulating reward emissions like in Axie
    function mintRewards(address to, uint256 amount) external onlyOwner {
        _mint(to, amount);
    }

    function mintBatch(address[] calldata recipients, uint256[] calldata amounts) external onlyOwner 
    {
        require(recipients.length == amounts.length, "Length mismatch");
        
        for (uint i = 0; i < recipients.length; i++) {
            _mint(recipients[i], amounts[i]);
        }
    }
}
