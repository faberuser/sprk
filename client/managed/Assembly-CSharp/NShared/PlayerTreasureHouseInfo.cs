namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerTreasureHouseInfo
{
	public int Index { get; set; }

	public int ClearCount { get; set; }

	public string ClearedTime { get; set; }

	public string UpdatedTime { get; set; }

	public string CreatedTime { get; set; }

	public string ResetTime { get; set; }

	public string NextResetTime { get; set; }
}
