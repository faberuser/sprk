namespace NShared;

[JsonSerializable(Javascript = true)]
public class ItemResultInfo
{
	public int ItemIndex { get; set; }

	public int AddCount { get; set; }

	public int NewCount { get; set; }

	public int AddBoosterCount { get; set; }

	public int AddNPCBoosterCount { get; set; }

	public int AddBonusAssignedItemPercent { get; set; }

	public byte Locked { get; set; }

	public bool IsFirstClearReward { get; set; }
}
