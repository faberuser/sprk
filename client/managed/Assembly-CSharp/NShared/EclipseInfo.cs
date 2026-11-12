namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseInfo
{
	public int MatchIndex { get; set; }

	public bool IsDeckSave { get; set; }

	public bool IsPlayEclipse { get; set; }

	public int MaxWaveIndex { get; set; }

	public int CurrentWaveIndex { get; set; }

	public int DeckIndex { get; set; }

	public int LastDeckIndex { get; set; }

	public string ExpireTime { get; set; }

	public StaminaResultInfo EclipseStaminaResult { get; set; }

	public CurrencyResultInfo3 EclipsePointResult { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public EclipseDeckResult[] DeckResults { get; set; }
}
