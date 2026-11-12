namespace NShared.RechargeStamina;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/recharge_stamina";

	public StaminaType StaminaType { get; set; }
}
