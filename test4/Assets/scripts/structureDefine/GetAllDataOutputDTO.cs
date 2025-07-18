using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;
using System.Collections.Generic;


[FunctionOutput]
public class GetAllDataOutputDTO : IFunctionOutputDTO
{
    [Parameter("uint256", "ethPrice", 1)]
    public BigInteger EthPrice { get; set; }

    [Parameter("tuple[]", "", 2)]
    public List<Score> Scores { get; set; }
}