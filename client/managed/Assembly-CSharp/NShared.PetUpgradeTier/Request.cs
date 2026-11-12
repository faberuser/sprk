namespace NShared.PetUpgradeTier;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/pet_upgrade_tier";

	public int[] PetSouls { get; set; }
}
