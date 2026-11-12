using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerShieldBegin : FastHandler<CallbackMessage<ShieldBegin>>
{
	protected override bool OnMessage(CallbackMessage<ShieldBegin> message)
	{
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}
}
