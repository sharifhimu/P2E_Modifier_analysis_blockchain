// truffle exec ./scripts/mintFew.js --network sepolia
const MyToken = artifacts.require("MyToken");
const OtherToken = artifacts.require("OtherToken");


module.exports = async function (callback) {
  try {
    const token = await MyToken.deployed();
    const web3 = token.constructor.web3;

    const owner = (await web3.eth.getAccounts())[0];

    // 1) List of recipient addresses (your 10 wallets)
    const alladdresses = [
    // "0x87dD058f9930103c7602F160448677Ff4b1954e9",
    // "0x62CAb5899170f6521Aa4878F64Ec3D89D9e75446",
    // "0xa3377A08008c0463dAde7b1C6DeE1372dE15F912",
    // "0x396c0A098db19f91c6a618FA4d553F4b413d042d",
    // "0x5A835BDa6Cb44225925b2adE9D3Bd2b5AaD26294",
    // "0xea594729D16C4BFeEE4F093d22f0d3f233642a6B",
    // "0x4ed7b73b053B2Ce4bA28BeAa91961CFa7Aab4498",
    // "0x37348f0166a4d77c7f09cdD2c947Bd69E70f913c",
    // "0x5d723a7016E8874B2525b7999667c2CEA08b9361",
    // "0xe0030baDBEBcbF0a2E8f1d64ad654B68a98A0231",
    // "0x3DfB5B6000927EE37D340057442B14E5161AA15a",
    // "0x7AD8EbeFC4Ff2a3ebc615CeB6fd5Eb23c17563E9",
    // "0x80b06e12ad6181d60c1bc72A9F2161c67C4585BF",
    // "0x2CdDa103e974c697aBe493a9324951FCDf8544d9",
    // "0x3917E83DC99cBe3D14C66b02c871E9Ce7cB7feF0",
    // "0x23Dd60fE5edFE2F8b1C7136Ac9187C9242AC6211",
    // "0x7f31Dd80Ee57E9CaC196B4CbA9C6f384B5b18F38",
    "0x31e68B8aaF070613E7a7Df9BBDdbD16a624d7447",
    "0xC594638Be91A5E34b971b9A6Ade8FCa3c49A39a8",
    "0x87C120aDCc05B364769dd0732B99b316135C7D6e"
    ];

    // const startIndex = 7;
    // const endIndex = 7;
    const addresses = alladdresses

    // 2) Target balances for each address (in *human* tokens, not wei)
    //    e.g. 1000 means "1000 tokens"
    const alltargetHuman = [
      // 10000,
      // 500,
      // 8000,
      // 1000,
      // 5000,
      // 150,
      // 10000,
      // 500,
      // 8000,
      // 1000,
      // 5000,
      // 150,
      // 10000,
      // 500,
      // 8000,
      // 1000,
      // 5000,
      150,
      10000,
      500
    ];

    const targetHuman = alltargetHuman

    if (targetHuman.length !== addresses.length) {
      throw new Error("addresses.length and targetHuman.length must match");
    }

    console.log("Using owner:", owner, "\n");

    let success = 0;
    let fail = 0;

    for (let i = 0; i < addresses.length; i++) {
      const addr = addresses[i];
      const target = targetHuman[i];

      // convert target to wei (18 decimals)
      const targetWei = web3.utils.toWei(target.toString(), "ether");

      // 1. check current balance
      const balWei = await token.balanceOf(addr);
      const balHuman = web3.utils.fromWei(balWei.toString(), "ether");

      console.log(
        `\n[${i}] Address: ${addr}\n` +
        `    Current: ${balHuman} tokens\n` +
        `    Target : ${target} tokens`
      );

      // if already at or above target, skip
      if (balWei.gte(web3.utils.toBN(targetWei))) {
        console.log("    -> already >= target, skipping");
        continue;
      }

      // 2. compute how much more is needed
      const diffWei = web3.utils
        .toBN(targetWei)
        .sub(balWei); // target - current

      const diffHuman = web3.utils.fromWei(diffWei.toString(), "ether");
      console.log(`    Need to add: ${diffHuman} tokens`);

      // 3. mint or transfer that amount
      try {
        // If your token is mintable from owner:
        const tx = await token.mintRewards(addr, diffWei, { from: owner });

        // If your token is fixed‑supply and you want to send from owner instead,
        // comment previous line and use:
        // const tx = await token.transfer(addr, diffWei, { from: owner });

        console.log(
          `    ✓ Tx: ${tx.tx} | Gas used: ${tx.receipt.gasUsed}`
        );
        success++;
      } catch (err) {
        console.log(`    ✗ Error: ${err.message}`);
        fail++;
      }
    }

    console.log("\n=== DONE ===");
    console.log("Success:", success);
    console.log("Failed :", fail);

    callback();
  } catch (err) {
    console.error("Script error:", err);
    callback(err);
  }
};
