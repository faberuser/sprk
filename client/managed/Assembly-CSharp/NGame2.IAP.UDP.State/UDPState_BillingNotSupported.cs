namespace NGame2.IAP.UDP.State;

public class UDPState_BillingNotSupported : StateBase
{
	public string Error = string.Empty;

	public override EStatus Status => EStatus.BillingNotSupported;

	public UDPState_BillingNotSupported(string error)
	{
		Error = error;
	}
}
