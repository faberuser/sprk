namespace NShared.NEvent;

[JsonSerializable]
public class BattleRemoveDeck : BaseNodeEvent
{
	public int RemoveHeroIndex { get; set; }
}
