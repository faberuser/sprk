namespace NShared.RechargeUndergroundPrisonKey;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "dow_dungeon/recharge_underground_prison_key";

	public int DungeonIndex { get; set; }
}
