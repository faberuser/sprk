using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerShieldEnd : FastHandler<CallbackMessage<ShieldEnd>>
{
	protected override bool OnMessage(CallbackMessage<ShieldEnd> message)
	{
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}
}
