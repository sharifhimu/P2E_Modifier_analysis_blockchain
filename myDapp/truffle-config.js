require('dotenv').config();
const HDWalletProvider = require('@truffle/hdwallet-provider');

module.exports = {
  networks: {
    sepolia: {
      provider: () => new HDWalletProvider(
        process.env.PRIVATE_KEY,
        process.env.RPC_URL
      ),
      network_id: 11155111,
      gas: 5500000,
      gasPrice: 20000000000, // 20 Gwei
      confirmations: 0,
      timeoutBlocks: 500,
      skipDryRun: true,
      networkCheckTimeout: 60000,
      production: false,
      disableConfirmationListener: true
    }
  },

  compilers: {
    solc: {
      version: "0.8.19"
    }
  }
};
