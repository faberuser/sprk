using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GachaSelectShopProbabilityData : ProbabilityBaseData
{
	[IgnoreMember]
	public override int Index
	{
		get
		{
			return SelectShopIndex;
		}
		set
		{
			SelectShopIndex = value;
		}
	}

	[Key(0)]
	public int SelectShopIndex { get; set; }

	public override string ToString()
	{
		return "ProbabilityData Index=" + Index;
	}
}
