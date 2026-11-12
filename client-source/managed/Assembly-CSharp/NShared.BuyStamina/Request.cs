namespace NShared.BuyStamina;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/buy_stamina";

	public StaminaType StaminaType { get; set; }
}
