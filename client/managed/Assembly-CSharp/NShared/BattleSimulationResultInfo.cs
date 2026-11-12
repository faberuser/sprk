namespace NShared;

[JsonSerializable(Javascript = true)]
public class BattleSimulationResultInfo
{
	public string GroupId { get; set; }

	public string Uid { get; set; }

	public bool IsWin { get; set; }

	public long RealTime { get; set; }

	public override string ToString()
	{
		return $"UID:{Uid},IsWin:{IsWin},RealTime:{RealTime}";
	}
}
