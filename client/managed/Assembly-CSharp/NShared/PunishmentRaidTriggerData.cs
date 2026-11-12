using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PunishmentRaidTriggerData : IDataKeySelector<int>
{
	[Key(0)]
	public int TriggerIndex { get; set; }

	[Key(1)]
	public string[] Condition { get; set; }

	[Key(2)]
	public int[] AffixIndices { get; set; }

	[Key(3)]
	public string IconColor { get; set; }

	[Key(4)]
	public string IconName { get; set; }

	int IDataKeySelector<int>.Key => TriggerIndex;

	public override string ToString()
	{
		return "PunishmentRaidTriggerData TriggerIndex=" + TriggerIndex;
	}
}
