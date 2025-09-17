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

Click "Open" → Navigate to /test4


### ✅ Step 3: Run the Game
In Unity: click ▶️ "Play"

Game should detect deployed contracts and allow interactions

### Video links of the running project
#### Minimal design project: https://hongik-my.sharepoint.com/:v:/g/personal/sharif_mail_hongik_ac_kr/ES7XGnz1KkhGrlXMSgVx1ksBuwvDfDE_St1wSFuwhn2Qbg?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=hxQN8C

#### EG-AMM: https://hongik-my.sharepoint.com/:v:/g/personal/sharif_mail_hongik_ac_kr/Ed_4zeNoub1Onkx4cNJtV40BIQYU7pclqnN4-NIDbFlWEA?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=AagcWy
