using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class BoosterDefineData : IDataKeySelector<string>
{
	[Key(0)]
	public string BoosterCode { get; set; }

	[Key(1)]
	public EventBonusType BonusType { get; set; }

	[Key(2)]
	public ECostumeBuffType CostumeBuffType { get; set; }

	[Key(3)]
	public string IconItemCode { get; set; }

	[Key(4)]
	public string TooltipName { get; set; }

	[Key(5)]
	public string TooltipDesc { get; set; }

	[Key(6)]
	public string PopUpBuffCategory { get; set; }

	[Key(7)]
	public string PopUpBuffDetail { get; set; }

	string IDataKeySelector<string>.Key => BoosterCode;

	public override string ToString()
	{
		return "BoosterCode=" + BoosterCode;
	}
}
