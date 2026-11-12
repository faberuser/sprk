namespace NShared;

[JsonSerializable(Javascript = true)]
public class BattleSimulationResultInfoWrapper
{
	public BattleSimulationResultInfo[] BattleSimulationResultInfos { get; set; }
}
