namespace NShared.B2M_GetSimulation;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2m_get_simulation";

	public string BattleSimulationResultInfos { get; set; }

	public int ReservedSimulationCount { get; set; }
}
