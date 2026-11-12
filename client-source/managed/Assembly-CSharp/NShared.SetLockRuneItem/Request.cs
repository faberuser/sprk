namespace NShared.SetLockRuneItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/set_lock_rune_item";

	public int RuneItemIndex { get; set; }

	public byte Locked { get; set; }
}
