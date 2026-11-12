using System.Collections.Generic;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class AccessoryPositionInfoPairs
{
	public Dictionary<int, PositionInfo> Container { get; set; }
}
