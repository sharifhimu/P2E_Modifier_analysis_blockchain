require('dotenv').config();
const HDWalletProvider = require('@truffle/hdwallet-provider');

module.exports = {
  networks: {
    sepolia: {
      provider: () => new HDWalletProvider({
        privateKeys: [process.env.PRIVATE_KEY],
        providerOrUrl: process.env.RPC_URL,
        pollingInterval: 15000,  // 15 seconds between requests
        chainId: 11155111
      }),
      network_id: 11155111,
      gas: 5500000,
      gasPrice: 20000000000,
      confirmations: 2,          // Wait for 2 confirmations
      timeoutBlocks: 500,
      skipDryRun: true,
      networkCheckTimeout: 120000,  // 2 minutes
      deploymentPollingInterval: 15000
    }
  },
  compilers: {
    solc: {
      version: "0.8.19"
    }
  }
};
