using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AdventureNoticeData : IDataKeySelector<string>
{
	[Key(0)]
	public string Code { get; set; }

	[Key(1)]
	public string[] Action { get; set; }

	[Key(2)]
	public StaminaType StaminaType { get; set; }

	[Key(3)]
	public string[] OpenCondition { get; set; }

	[Key(4)]
	public int TutorialIndex { get; set; }

	[Key(5)]
	public string DisableText { get; set; }

	[Key(6)]
	public bool EnableEvent { get; set; }

	[Key(7)]
	public int NodeChapterIndex { get; set; }

	[Key(8)]
	public int NodeDungeonIndex { get; set; }

	[Key(9)]
	public BattleType BattleType { get; set; }

	[Key(10)]
	public bool UseLocalTime { get; set; }

	string IDataKeySelector<string>.Key => Code;
}
