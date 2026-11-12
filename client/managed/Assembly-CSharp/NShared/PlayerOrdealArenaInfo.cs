namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerOrdealArenaInfo
{
	public int SeasonIndex { get; set; }

	public string SelectedTierType { get; set; }

	public int RefreshCount { get; set; }

	public string DeadHeroInfo { get; set; }

	public string BattleNodeIndices { get; set; }

	public string EventNodeIndices { get; set; }

	public string ClearNodeIndices { get; set; }

	public string SelectedBuffIndices { get; set; }

	public string SelectBuffIndices { get; set; }

	public int Score { get; set; }
}
