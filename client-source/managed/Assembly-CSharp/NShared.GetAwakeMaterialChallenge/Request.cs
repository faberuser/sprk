namespace NShared.GetAwakeMaterialChallenge;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "trial_dungeon/get_awake_material_challenge";

	public int HeroIndex { get; set; }
}
