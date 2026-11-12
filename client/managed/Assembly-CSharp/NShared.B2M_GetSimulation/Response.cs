namespace NShared.B2M_GetSimulation;

[BattlePacket]
public class Response
{
	public ResultType Result { get; set; }

	public string[] RemoveGroupIds { get; set; }

	public BattleSimulationInfo[] BattleSimulationInfos { get; set; }
}
