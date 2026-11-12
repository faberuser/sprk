namespace NShared;

[JsonSerializable]
public class BattleCreatureStockData
{
	public CreatureData CreatureData { get; set; }

	public BattleCreatureCreateInfo CreateInfo { get; set; }

	public ResultCreatureInfo InitialInfo { get; set; }

	public long PlayerId { get; set; }

	public long CreatedTime { get; set; }
}
