namespace NShared;

[JsonSerializable(Javascript = true)]
public class NPCFriendlyInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => HeroIndex;

	public int HeroIndex { get; set; }

	public int Step { get; set; }

	public int FriendlyPoint { get; set; }

	public int EndRewardCount { get; set; }
}
