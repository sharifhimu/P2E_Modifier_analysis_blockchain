# 🧩 Unity + Truffle => P2E tokenomies Project

This repository combines a **Unity game frontend** and a **Truffle-based smart contract system** to simulate a modifier-driven Play-to-Earn (P2E) economy. It is designed to run locally using Ganache.

---

## 📁 Project Structure
### /test4/ <- Unity frontend
### /myDapp/ <- Smart contracts using Truffle

---

## ⚙️ Prerequisites

- [Node.js](https://nodejs.org/)
- [Truffle](https://trufflesuite.com/)
- [Ganache GUI or CLI](https://trufflesuite.com/ganache/)
- [Unity Hub + Unity Editor](https://unity.com/)

---



## 🔗 Part 1: Run Smart Contracts on Local Ganache

### ✅ Step 1: Start Ganache

Choose **one**:

#### 🟦 Option A: Ganache GUI

1. Open the app
2. Start a new workspace
3. Note the port (usually **8545**)

#### 🟧 Option B: Ganache CLI
```bash
ganache-cli --port 8545
```
### Step 2: Configure Truffle

Edit truffle-config.js in /TruffleContracts/ to match your Ganache port.
For Ganache GUI/CLI:
networks: {
  development: {
    host: "127.0.0.1",
    port: 8545,
    network_id: "*"
  }
}

### Step 3: Compile and Migrate Contracts

```bash
cd myDapp
npm install
truffle compile
truffle migrate --network development
```

### Step 4: smart contract addresses and ABI
1. Open Ganache
2. Go to "CONTRACTS"
3. You will see 3 contracts: TokenPair, MyToken, OtherToken
4. Save their address
5. After deploy, ABI json are created in the .json file.
6. Go to myDapp/build/contracts
7. You can see 3 .json file: TokenPair.json, MyToken.json, OtherToken.json
8. Open each of them.
9. You will see a property named "abi"
10. Copy the abi property from 3 of those files


## Part 2: Run the Unity Game
### ✅ Step 1: Open Project
Launch Unity Hub

Click "Open" → Navigate to /test4

### Step 2: define the contracts address and abi into the game
1. Open ABIManager.cs.
2. You can see 3 variables: tokenpairAddress, mytokenAddress, othertokenAddress
3. Paste the addresses from the contract in these variables as the name matches
4. Also, go to Assets/Resources.
5. You can see 3 json file: TokenPair.json, MyToken.json, OtherToken.json
6. Paste the copied abi into those files according to the name 

### ✅ Step 3: Run the Game
In Unity: click ▶️ "Play"


### Video links of the running project
#### Minimal design project: https://hongik-my.sharepoint.com/:v:/g/personal/sharif_mail_hongik_ac_kr/ES7XGnz1KkhGrlXMSgVx1ksBuwvDfDE_St1wSFuwhn2Qbg?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=hxQN8C

#### EG-AMM: https://hongik-my.sharepoint.com/:v:/g/personal/sharif_mail_hongik_ac_kr/Ed_4zeNoub1Onkx4cNJtV40BIQYU7pclqnN4-NIDbFlWEA?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=AagcWy
