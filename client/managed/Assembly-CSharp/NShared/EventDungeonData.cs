using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class EventDungeonData : IDataKeySelector<EventDungeonKey>
{
	EventDungeonKey IDataKeySelector<EventDungeonKey>.Key => new EventDungeonKey(ChapterIndex, DungeonIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public int ShowIndex { get; set; }

	[Key(3)]
	public string DecorationSelectorName { get; set; }

	[Key(4)]
	public string PrefabName { get; set; }

	[Key(5)]
	public EventDungeonPropType[] PropTypes { get; set; }

	[Key(6)]
	public EEventDungeonCloseType CloseType { get; set; }

	[Key(7)]
	public string OpenType1 { get; set; }

	[Key(8)]
	public string[] OpenTypeValues1 { get; set; }

	[Key(9)]
	public string OpenType2 { get; set; }

	[Key(10)]
	public string[] OpenTypeValues2 { get; set; }

	[Key(11)]
	public int EventDungeonPointRate_Easy { get; set; }

	[Key(12)]
	public int EventDungeonPointAmount_Easy { get; set; }

	[Key(13)]
	public int EventDungeonPointRate_Normal { get; set; }

	[Key(14)]
	public int EventDungeonPointAmount_Normal { get; set; }

	[Key(15)]
	public int EventDungeonPointRate_Hard { get; set; }

	[Key(16)]
	public int EventDungeonPointAmount_Hard { get; set; }

	[Key(17)]
	public int EventDungeonPointRate_Hell { get; set; }

	[Key(18)]
	public int EventDungeonPointAmount_Hell { get; set; }

	[Key(19)]
	public int EventDungeonPoint2Rate_Easy { get; set; }

	[Key(20)]
	public int EventDungeonPoint2Amount_Easy { get; set; }

	[Key(21)]
	public int EventDungeonPoint2Rate_Normal { get; set; }

	[Key(22)]
	public int EventDungeonPoint2Amount_Normal { get; set; }

	[Key(23)]
	public int EventDungeonPoint2Rate_Hard { get; set; }

	[Key(24)]
	public int EventDungeonPoint2Amount_Hard { get; set; }

	[Key(25)]
	public int EventDungeonPoint2Rate_Hell { get; set; }

	[Key(26)]
	public int EventDungeonPoint2Amount_Hell { get; set; }

	[Key(27)]
	public string PortraitPath { get; set; }

	[Key(28)]
	public string DungeonName { get; set; }

	[Key(29)]
	public int[] ResetAmount { get; set; }

	[Key(30)]
	public int MaxResetCount { get; set; }

	[Key(31)]
	public string ResetCurrencyType { get; set; }

	[Key(32)]
	public string[] HelpTexts { get; set; }

	[Key(33)]
	public string OpenedDungeonDesc { get; set; }

	[Key(34)]
	public string ClosedDungeonDesc { get; set; }

	[Key(35)]
	public int BanIndex { get; set; }

	[Key(36)]
	public int RewardIndex { get; set; }

	[Key(37)]
	public string DungeonCardPrefix { get; set; }

	[Key(38)]
	public string DungeonCardName { get; set; }

	public override string ToString()
	{
		return "EventDungeonData ChapterIndex=" + ChapterIndex + ", DungeonIndex=" + DungeonIndex;
	}
}
