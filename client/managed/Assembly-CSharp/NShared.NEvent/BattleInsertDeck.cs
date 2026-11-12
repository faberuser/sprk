namespace NShared.NEvent;

[JsonSerializable]
public class BattleInsertDeck : BaseNodeEvent
{
	public bool IsMain { get; set; }

	public CachedHeroInfo InsertHeroInfo { get; set; }
}
