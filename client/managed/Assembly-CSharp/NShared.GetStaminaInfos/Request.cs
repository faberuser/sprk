namespace NShared.GetStaminaInfos;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/get_stamina_infos";

	public StaminaType[] StaminaTypes { get; set; }
}
