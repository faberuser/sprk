using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SkillPriceData : IDataKeySelector<SkillPriceKey>
{
	SkillPriceKey IDataKeySelector<SkillPriceKey>.Key => new SkillPriceKey(SlotIndex, Level);

	[Key(0)]
	public int SlotIndex { get; set; }

	[Key(1)]
	public int Level { get; set; }

	[Key(2)]
	public int ReqGold { get; set; }

	[Key(3)]
	public int ReqItemCount { get; set; }

	public override string ToString()
	{
		return "SlotIndex=" + SlotIndex + ", Level=" + Level + ", ReqGold=" + ReqGold;
	}
}
