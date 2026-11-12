namespace NShared.GetStamina;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/get_stamina";

	public StaminaType StaminaType { get; set; }
}
