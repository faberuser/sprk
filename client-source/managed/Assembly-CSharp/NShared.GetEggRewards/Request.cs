namespace NShared.GetEggRewards;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/get_egg_rewards";

	public int SlotIndex { get; set; }

	public int IncubatorIndex { get; set; }

	public int ItemIndex { get; set; }
}
