using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureStarData : IDataKeySelector<CreatureStarDataKey>
{
	CreatureStarDataKey IDataKeySelector<CreatureStarDataKey>.Key => new CreatureStarDataKey(Star, Transcended);

	[Key(0)]
	public int Star { get; set; }

	[Key(1)]
	public int Transcended { get; set; }

	[Key(2)]
	public int GetHeroTeamExp { get; set; }

	[Key(3)]
	public int AwakeHeroTeamExp { get; set; }

	[Key(4)]
	public int MaxHeroLevel { get; set; }

	[Key(5)]
	public int HeroAwakeItemCount { get; set; }

	[Key(6)]
	public int HeroTranscendItemIndex { get; set; }

	[Key(7)]
	public int HeroTranscendItemCount { get; set; }

	[Key(8)]
	public int MaxPurified { get; set; }

	[Key(9)]
	public int Gold { get; set; }

	[Key(10)]
	public int PurifyAmount1 { get; set; }

	[Key(11)]
	public int Rate1 { get; set; }

	[Key(12)]
	public int PurifyAmount2 { get; set; }

	[Key(13)]
	public int Rate2 { get; set; }

	[Key(14)]
	public int PurifyAmount3 { get; set; }

	[Key(15)]
	public int Rate3 { get; set; }

	[Key(16)]
	public int ExchangeItemCount { get; set; }

	[Key(17)]
	public int ExchangeTranscendedItemCount { get; set; }

	[Key(18)]
	public int AllOwnedRewardIndex { get; set; }

	public override string ToString()
	{
		return " Star=" + Star + " GetTeamExp=" + GetHeroTeamExp;
	}
}
