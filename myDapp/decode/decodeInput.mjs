import Web3 from 'web3';
const web3 = new Web3();

// ABI for the contract function
// Note: This is a simplified ABI for the function you provided.
const abi = [
    {
      "anonymous": false,
      "inputs": [
        {
          "indexed": false,
          "internalType": "address",
          "name": "sender",
          "type": "address"
        },
        {
          "indexed": false,
          "internalType": "uint256",
          "name": "playerId",
          "type": "uint256"
        },
        {
          "indexed": false,
          "internalType": "string",
          "name": "wallet",
          "type": "string"
        },
        {
          "indexed": false,
          "internalType": "uint256",
          "name": "coin",
          "type": "uint256"
        },
        {
          "indexed": false,
          "internalType": "uint256",
          "name": "character",
          "type": "uint256"
        }
      ],
      "name": "ScoreSubmitted",
      "type": "event"
    },
    {
      "inputs": [],
      "name": "ethPrice",
      "outputs": [
        {
          "internalType": "uint256",
          "name": "",
          "type": "uint256"
        }
      ],
      "stateMutability": "view",
      "type": "function",
      "constant": true
    },
    {
      "inputs": [
        {
          "internalType": "uint256",
          "name": "",
          "type": "uint256"
        }
      ],
      "name": "playerAddresses",
      "outputs": [
        {
          "internalType": "address",
          "name": "",
          "type": "address"
        }
      ],
      "stateMutability": "view",
      "type": "function",
      "constant": true
    },
    {
      "inputs": [
        {
          "internalType": "address",
          "name": "",
          "type": "address"
        }
      ],
      "name": "playerScores",
      "outputs": [
        {
          "internalType": "uint256",
          "name": "playerId",
          "type": "uint256"
        },
        {
          "internalType": "string",
          "name": "walletAddress",
          "type": "string"
        },
        {
          "internalType": "uint256",
          "name": "coinAmount",
          "type": "uint256"
        },
        {
          "internalType": "uint256",
          "name": "characterIndex",
          "type": "uint256"
        },
        {
          "internalType": "uint256",
          "name": "exchangedAmount",
          "type": "uint256"
        },
        {
          "internalType": "bool",
          "name": "exists",
          "type": "bool"
        }
      ],
      "stateMutability": "view",
      "type": "function",
      "constant": true
    },
    {
      "inputs": [],
      "name": "hello",
      "outputs": [
        {
          "internalType": "string",
          "name": "",
          "type": "string"
        }
      ],
      "stateMutability": "pure",
      "type": "function",
      "constant": true
    },
    {
      "inputs": [
        {
          "internalType": "uint256",
          "name": "_price",
          "type": "uint256"
        }
      ],
      "name": "setPrice",
      "outputs": [],
      "stateMutability": "nonpayable",
      "type": "function"
    },
    {
      "inputs": [
        {
          "internalType": "uint256",
          "name": "playerId",
          "type": "uint256"
        },
        {
          "internalType": "string",
          "name": "add",
          "type": "string"
        },
        {
          "internalType": "uint256",
          "name": "coinAmount",
          "type": "uint256"
        },
        {
          "internalType": "uint256",
          "name": "characterIndex",
          "type": "uint256"
        }
      ],
      "name": "Sendscore",
      "outputs": [],
      "stateMutability": "nonpayable",
      "type": "function"
    },
    {
      "inputs": [
        {
          "internalType": "uint256",
          "name": "_newCoinAmount",
          "type": "uint256"
        },
        {
          "internalType": "uint256",
          "name": "_newCharacterIndex",
          "type": "uint256"
        },
        {
          "internalType": "uint256",
          "name": "_exchangedAmount",
          "type": "uint256"
        }
      ],
      "name": "updateScoreByPlayerId",
      "outputs": [],
      "stateMutability": "nonpayable",
      "type": "function"
    },
    {
      "inputs": [
        {
          "internalType": "uint256",
          "name": "_amount",
          "type": "uint256"
        }
      ],
      "name": "setExchangeAmount",
      "outputs": [],
      "stateMutability": "nonpayable",
      "type": "function"
    },
    {
      "inputs": [],
      "name": "getAllData",
      "outputs": [
        {
          "internalType": "uint256",
          "name": "",
          "type": "uint256"
        },
        {
          "components": [
            {
              "internalType": "uint256",
              "name": "playerId",
              "type": "uint256"
            },
            {
              "internalType": "string",
              "name": "walletAddress",
              "type": "string"
            },
            {
              "internalType": "uint256",
              "name": "coinAmount",
              "type": "uint256"
            },
            {
              "internalType": "uint256",
              "name": "characterIndex",
              "type": "uint256"
            },
            {
              "internalType": "uint256",
              "name": "exchangedAmount",
              "type": "uint256"
            },
            {
              "internalType": "bool",
              "name": "exists",
              "type": "bool"
            }
          ],
          "internalType": "struct TestContract.Score[]",
          "name": "",
          "type": "tuple[]"
        }
      ],
      "stateMutability": "view",
      "type": "function",
      "constant": true
    },
    {
      "inputs": [
        {
          "internalType": "address",
          "name": "_wallet",
          "type": "address"
        }
      ],
      "name": "getPlayerData",
      "outputs": [
        {
          "internalType": "uint256",
          "name": "playerId",
          "type": "uint256"
        },
        {
          "internalType": "string",
          "name": "walletAddress",
          "type": "string"
        },
        {
          "internalType": "uint256",
          "name": "coinAmount",
          "type": "uint256"
        },
        {
          "internalType": "uint256",
          "name": "characterIndex",
          "type": "uint256"
        },
        {
          "internalType": "uint256",
          "name": "exchangedAmount",
          "type": "uint256"
        },
        {
          "internalType": "bool",
          "name": "exists",
          "type": "bool"
        }
      ],
      "stateMutability": "view",
      "type": "function",
      "constant": true
    }
  ]

// Transaction input data 
const inputData = "0x5a08e604000000000000000000000000000000000000000000000000000000000000002d";

// Extract the selector
// Remove function selector (first 4 bytes = 8 hex chars + "0x" = 10 chars)
const selector = inputData.slice(0, 10); // 0x + 4 bytes
const dataWithoutSelector = inputData.slice(10); // Remove '0x' and 4-byte selector


// Parameter types for Sendscore(uint256,string,uint256,uint256)
// const types = ['uint256', 'string', 'uint256', 'uint256'];

// Parameter types for setExchangeAmount(uint256)
const types = ['uint256'];

// Decode parameters
const decodedParams = web3.eth.abi.decodeParameters(types, dataWithoutSelector);

console.log("Decoded Params: ", decodedParams );


// Compare with all functions in ABI
for (const item of abi) {
  if (item.type === 'function') {
    const sig = `${item.name}(${item.inputs.map(i => i.type).join(',')})`;
    const hash = web3.utils.keccak256(sig).slice(0, 10);
    if (hash === selector) {
      console.log("✅ Matched Function Signature:", sig);
      break;
    }
  }
}