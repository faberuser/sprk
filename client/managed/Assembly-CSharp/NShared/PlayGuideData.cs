using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PlayGuideData : IDataKeySelector<int>
{
	[JsonSerializable(Javascript = true)]
	public enum GuideType
	{
		None,
		UseShop,
		UseContent
	}

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public GuideType Type { get; set; }

	[Key(2)]
	public string[] Condition1 { get; set; }

	[Key(3)]
	public string[] Condition2 { get; set; }

	[Key(4)]
	public string[] Condition3 { get; set; }

	[Key(5)]
	public int TutorialIndex { get; set; }

	[Key(6)]
	public string TextKey { get; set; }

	[Key(7)]
	public string MoveTaskType { get; set; }

	[Key(8)]
	public string[] MoveTaskValue { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
