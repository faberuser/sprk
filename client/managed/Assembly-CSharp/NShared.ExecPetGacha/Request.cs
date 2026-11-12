namespace NShared.ExecPetGacha;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/exec_pet_gacha";

	public int GachaIndex { get; set; }

	public int ItemIndex { get; set; }

	public bool Free { get; set; }
}
