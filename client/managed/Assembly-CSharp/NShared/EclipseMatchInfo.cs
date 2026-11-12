namespace NShared;

public class EclipseMatchInfo : MatchInfo
{
	public int MatchIndex { get; set; }

	public int DeckIndex { get; set; }

	public int WaveIndex { get; set; }

	public int MaxWaveIndex { get; set; }

	public int CurrentCreatureLevel { get; set; }

	public string GameServerSessionKey { get; set; }

	public EclipseDeckData[] DeckList { get; set; }

	public EclipseDungeonData[] DungeonList { get; set; }

	public bool IsContinuousBattle { get; set; }
}
