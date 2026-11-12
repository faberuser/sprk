using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class TutorialData : IDataKeySelector<TutorialKey>
{
	TutorialKey IDataKeySelector<TutorialKey>.Key => new TutorialKey(Index, Sequence);

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Sequence { get; set; }

	[Key(2)]
	public int ReqIndex { get; set; }

	[Key(3)]
	public string Command { get; set; }

	[Key(4)]
	public string[] Argument { get; set; }

	[Key(5)]
	public int RewardGold { get; set; }

	[Key(6)]
	public int RewardGem { get; set; }

	[Key(7)]
	public int RewardIndex { get; set; }

	[Key(8)]
	public string[] RewardAction { get; set; }

	[Key(9)]
	public int[] RewardDungeonIndex { get; set; }

	[Key(10)]
	public bool HideRewardPopup { get; set; }

	[Key(11)]
	public bool Repeatable { get; set; }

	[Key(12)]
	public int SendAdjustEventIndex { get; set; }

	public override string ToString()
	{
		return "[" + Index + "," + Sequence + "] = " + Command;
	}
}
