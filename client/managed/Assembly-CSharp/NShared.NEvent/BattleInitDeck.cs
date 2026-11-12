namespace NShared.NEvent;

[JsonSerializable]
public class BattleInitDeck : BaseNodeEvent
{
	public CachedHeroInfo[] MainHeroInfos { get; set; }

	public CachedHeroInfo[] SubHeroInfos { get; set; }
}
