namespace NShared.PetGachaRoofReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/pet_gacha_roof_reward";

	public int GachaIndex { get; set; }
}
