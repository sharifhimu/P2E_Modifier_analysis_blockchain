using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;
using System.Collections.Generic;



[FunctionOutput]
public class Reserves : IFunctionOutputDTO
{
    [Parameter("uint112", "reserve0", 1)]
    public BigInteger Reserve0 { get; set; }

    [Parameter("uint112", "reserve1", 2)]
    public BigInteger Reserve1 { get; set; }

    [Parameter("uint32", "blockTimestampLast", 3)]
    public uint BlockTimestampLast { get; set; }
}