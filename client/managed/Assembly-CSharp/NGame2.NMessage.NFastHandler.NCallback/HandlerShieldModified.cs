using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerShieldModified : FastHandler<CallbackMessage<ShieldModified>>
{
	protected override bool OnMessage(CallbackMessage<ShieldModified> message)
	{
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}
}
