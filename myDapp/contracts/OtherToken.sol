// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract OtherToken is ERC20, Ownable {
    constructor(uint256 initialSupply) ERC20("OtherToken", "OTK") {
        _mint(msg.sender, initialSupply);
    }
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
