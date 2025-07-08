# 🧩 Unity + Truffle Blockchain Integration Project

This repository combines a **Unity game frontend** and a **Truffle-based smart contract system** to simulate a modifier-driven Play-to-Earn (P2E) economy. It is designed to run locally using Ganache and MetaMask.

---

## 📁 Project Structure
/UnityProject/ <- Unity frontend
/TruffleContracts/ <- Smart contracts using Truffle

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

## Part 2: Run the Unity Game
### ✅ Step 1: Open Project
Launch Unity Hub

Click "Open" → Navigate to /UnityProject/

### ✅ Step 2: Verify Web3 Integration
Ensure scripts use Web3, Nethereum, or similar

If you use WebGL, connect to MetaMask using injected Web3

### ✅ Step 3: Run the Game
In Unity: click ▶️ "Play"

Game should detect deployed contracts and allow interactions

