const MyToken = artifacts.require("MyToken");
const OtherToken = artifacts.require("OtherToken");

module.exports = async function(callback) {
    try {
        const myToken = await OtherToken.deployed();
        const accounts = await web3.eth.getAccounts();
        const owner = accounts[0];
        
    const addresses = [
        "0x87dD058f9930103c7602F160448677Ff4b1954e9",
        "0x62CAb5899170f6521Aa4878F64Ec3D89D9e75446",
        "0xa3377A08008c0463dAde7b1C6DeE1372dE15F912",
        "0x396c0A098db19f91c6a618FA4d553F4b413d042d",
        "0x5A835BDa6Cb44225925b2adE9D3Bd2b5AaD26294",
        "0xea594729D16C4BFeEE4F093d22f0d3f233642a6B",
        "0x4ed7b73b053B2Ce4bA28BeAa91961CFa7Aab4498",
        "0x37348f0166a4d77c7f09cdD2c947Bd69E70f913c",
        "0x5d723a7016E8874B2525b7999667c2CEA08b9361",
        "0xe0030baDBEBcbF0a2E8f1d64ad654B68a98A0231",
        "0x3DfB5B6000927EE37D340057442B14E5161AA15a",
        "0x7AD8EbeFC4Ff2a3ebc615CeB6fd5Eb23c17563E9",
        "0x80b06e12ad6181d60c1bc72A9F2161c67C4585BF",
        "0x2CdDa103e974c697aBe493a9324951FCDf8544d9",
        "0x3917E83DC99cBe3D14C66b02c871E9Ce7cB7feF0",
        "0x23Dd60fE5edFE2F8b1C7136Ac9187C9242AC6211",
        "0x7f31Dd80Ee57E9CaC196B4CbA9C6f384B5b18F38",
        "0x31e68B8aaF070613E7a7Df9BBDdbD16a624d7447",
        "0xC594638Be91A5E34b971b9A6Ade8FCa3c49A39a8",
        "0x87C120aDCc05B364769dd0732B99b316135C7D6e",
        "0xde20A3FA43D5724A7cC74B7665a1917be372dFEa",
        "0xBeB01Df53BB3C319f4e6efF6d8fFBF1E1C69125E",
        "0x8a86CB1c34D577aE051Ed3E7d6Ccd778B97c709e",
        "0x9Bfd92d82d62C34fFc64560FD26A18E1a6f37825",
        "0x3a9a04F946581a7197D3dEB10BA22767c34feFB4",
        "0x6254d7E379EfAd9005E460A96fB21f3AB0CCdd95",
        "0x490a0Aa40D500fEF4626a9e2C3508b9C54f66411",
        "0x7a2017d399C29A8B17097BDA53FBaa710783f16d",
        "0x9AeDFc90782b2979af94bd451C9b302E21b26919",
        "0xE61cC425A00F7006a0a3AC18A7D0c6CF9272E182",
        "0x6A3ACb38C04E118D13AffBe2343650e4E67a0Dd7",
        "0x11E6c65761F49993C1c4b4d3e713Ca8b3E64E856",
        "0xfe0db27D7f1bAF73B0864247E5644BbF2cd71bc3",
        "0x8A7A9F0fDB2dD571E59d9a4349934015618ED866",
        "0x351b3A7f768070deF8700cCF1f313003D5Dc3Ad4",
        "0x41ebf6b85489F65de16e9683A3F68F486423CA06",
        "0x0d4636f8dD45331F44Bcff2E2Fc04954BF0E4b36",
        "0xE5fdb0Ffc9080e29928834f89b817a633fEd39D7",
        "0x762f964cE7E97FF7978b6e9E40b832B20bB2C85d",
        "0x2a6775Dc6e9f22bf75d9a1A404E65Bfa4bE54BE3",
        "0xAcFd92253302976cA3C70a0d31807A04561a4a40",
        "0x1696AdBeACf45463e42cd8450e0f067a81B7B33b",
        "0x50e03EfB13ceBF24CC1F7175f59F4757b969474e",
        "0xbfeeFE69c2A50aB6550745D76631B441685648c4",
        "0x1138F1953b81e6417420E121542d4d5c32f4f943",
        "0x27ab0F0B9093Bbc4bfF4C2297908563baE7c7658",
        "0x4d6A3CC20a460895C9b4625f59cf332946B03eef",
        "0x4D2f30930A9C4cC279d54B20C3fd5515280dF891",
        "0xd7039abD0ef729fB14462177B55d4E632cA58d13",
        "0x3E699877F529FB2159632662dAF50D660681AF95",
        "0xF907eb57a4021bEf199b46396167a79113025dbd",
        "0xFeba66Eb27eE30E962a26fDF26952D2903eF9e9E",
        "0x74b01A909F9951F444CF42F2f4ca2a7ed2fA79c6",
        "0xF722706Bf9509022AB75F64472993cCe5fF8f736",
        "0xBb13B8dCa3561c89ad6538246EE715cCFE8fe9A7",
        "0xebc69bC679A2BF3CAa577032905ae04EA64c8557",
        "0xa64f6e8fec4FA28594418D5b28a0F0aA320c1F6f",
        "0xD5f08E61D6f38a11B8cbB332D5C74973Dba6126B",
        "0x05aBC43B229671a5f087A4be270B8dE567623CBC",
        "0x6609Fe8BB038Ec474a05eEE3FFb4106Ccd22FdaA",
        "0x8f7D7aCe6daBa977487b622B410C0CBEd3BC579f",
        "0x62f4d2A3a461A3566A8913153b39340A129BEa26",
        "0xcaBFcF8874e072C909Cd6f7d136CbC25Afb121D8",
        "0x683cfd1C72892C1eeb93ae2995cC211dD7Bc379b",
        "0x66Bd2178c32f14456275F1192ef88a15D32B4E17",
        "0x3De4a3909e11cf149e25D17AC3B2Ffa3B90f6b7F",
        "0x7b7Eaf6577603Eeb8660104eAd6532Ee2f9f24c5",
        "0x2240B8C5dEF0fd1D0a585eBc219c855a5FDbFd44",
        "0x091d41CdC8acd98012d550F738811463a6E6740d",
        "0xCc99eF33134dd08613EcB47dD3414Fb33A754720",
        "0x8128DAd1e0a4f7e043aEB64FD27c697b219cdE29",
        "0x6F302fa104f7d8a832F19B30acf5C8b24457BFba",
        "0xF91b12BcB7e6E6C6946A13e3700BFF1d8a2fe43d",
        "0x0B0862f320B04f1Ea79552e0aDb248CCD7A96Bd2",
        "0xbd558B2FC6a81175bbf77F4AAa596502B1B2e7Fa",
        "0x11931497115e2a57556092A563c659B1B530faf1",
        "0x56Ee005082c2226202732B2EF329EcDFa6F0981E",
        "0x982a4C03ee318D55933816a6BE7FF3B4ae9c9EE4",
        "0x598C6EaCc2fBe5E0da722A404aAA60EDF532Df29",
        "0x94a7F8218b2Bf8af7Cfb69aC748c46553bf9DAB3",
        "0x46Cf1b6B8c61aB88eA68505493f9EbEA131C69d3",
        "0x947ED89225b2646e9273e9019c531a3BBD0e5e52",
        "0xD3A3d6dF589ac12687A4213d8ffe623A1466F0A7",
        "0x417c6e52193Cd07f22729Faf0d918613D3fb6CEf",
        "0x3ce39e3a702F864ff1B41f80b4BAEB9dBe21b4c7",
        "0x6DD701138B50CA582EFcDE80aE32b9f12396eaD8",
        "0xFEFC1c9e8b2d3Dd095dDD71Dc5BC50F85e858Bb7",
        "0x765Da8579Cb46E6ca3ab36186F767e059F0aD619",
        "0xE96e635B52E492b970B1C672885f2105A006a7c1",
        "0x4AF78DaeC4E1157C4846092B4Ccb6DE439721422",
        "0x7B86aae182F74dF8f90217a4525aAcBC9568321E",
        "0x4f3883F618c7e94F91493939102a813D7193b65B",
        "0xA6Dae148E4B86226146A164E77cB705965200e3e",
        "0x84bf8C60F93b42EAf9F823447C6D8eBa9b1C0D81",
        "0x7cD299cd5451F87DA58bfE251fb24eB8E96D4170",
        "0x97A87C557926B23EA73bD42851D49ca5A28BcD8D",
        "0x7c18ed64Ea83aD347A90AD7D028D791D6681FdF5",
        "0xdafdaC2Fa2f5e0E01f35efcA09e78A4Db784B1a8",
        "0x8B8B5B6bD84aAc1D8d32c0C03D3D6a5961f9815E",
        "0x0786Dc9021b9f2662c75529610fa2Cd90C9d63eD"
    ]
        
        // which address to start
        // const startIndex = 24;
        const endIndex = 10;
        const remainingAddresses = addresses.slice(0, endIndex);
        
        console.log(`📊 Minting to ${remainingAddresses.length} addresses (skip first ${endIndex})\n`);

        const batchSize = 10;
        let totalMinted = 0;
        let successCount = 0;
        let failureCount = 0;
        let batchNum = 0;

        for (let batch = 0; batch < remainingAddresses.length; batch += batchSize) {
            batchNum++;
            const batchAddresses = remainingAddresses.slice(batch, batch + batchSize);
            const amounts = [];
            
            let batchTotal = 0;
            for (let addr of batchAddresses) {
                const randomTokens = Math.floor(Math.random() * 9900 + 100);
                const amount = web3.utils.toWei(randomTokens.toString(), "ether");
                amounts.push(amount);
                batchTotal += randomTokens;
                totalMinted += randomTokens;
            }
            
            console.log(`📦 Batch ${batchNum}: Minting ${batchAddresses.length} addresses (${batchTotal} tokens total)`);
            
            try {
                // Call mintBatch
                const tx = await myToken.mintBatch(batchAddresses, amounts, { from: owner });
                console.log(`   ✅ Tx: ${tx}`);
                successCount++;
                
                // Just log the tx hash, don't wait for receipt
                console.log(`   Gas will be deducted from owner account\n`);
                
            } catch (error) {
                failureCount++;
                console.error(`   ❌ Batch failed: ${error.message}\n`);
            }
            
            // Wait between batches
            if (batch + batchSize < remainingAddresses.length) {
                console.log(`⏳ Waiting 10 seconds before next batch...\n`);
                await new Promise(resolve => setTimeout(resolve, 10000));
            }
        }

        console.log(`\n✅ COMPLETE!`);
        console.log(`   Total batches: ${batchNum}`);
        console.log(`   Successful: ${successCount}`);
        console.log(`   Failed: ${failureCount}`);
        console.log(`   Total tokens: ${totalMinted.toFixed(0)}`);
        console.log(`   💰 Final cost: ~0.15917008 ETH (already spent)\n`);

        callback();
    } catch (error) {
        console.error("❌ Error:", error.message);
        callback(error);
    }
};