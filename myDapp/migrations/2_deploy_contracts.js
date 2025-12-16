const MyToken = artifacts.require("MyToken");
const OtherToken = artifacts.require("OtherToken");
const TokenPair = artifacts.require("TokenPair");

module.exports = async function (deployer, network, accounts) {
  // Deploy MyToken with initial supply (1 million tokens)
  const initialSupply1 = web3.utils.toWei('100000', 'ether');
  await deployer.deploy(MyToken, initialSupply1);
  const myToken = await MyToken.deployed();

  // Deploy OtherToken with initial supply (1 million tokens)
  const initialSupply2 = web3.utils.toWei('100000', 'ether');
  await deployer.deploy(OtherToken, initialSupply2);
  const otherToken = await OtherToken.deployed();

  // Deploy TokenPair with token addresses
  await deployer.deploy(TokenPair, myToken.address, otherToken.address);
  const tokenPair = await TokenPair.deployed();

  console.log("MyToken deployed at:", myToken.address);
  console.log("OtherToken deployed at:", otherToken.address);
  console.log("TokenPair deployed at:", tokenPair.address);
};
