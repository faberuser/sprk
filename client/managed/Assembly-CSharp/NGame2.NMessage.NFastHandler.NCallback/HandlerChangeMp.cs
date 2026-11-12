using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerChangeMp : FastHandler<CallbackMessage<ChangeMp>>
{
	protected override bool OnMessage(CallbackMessage<ChangeMp> message)
	{
		return true;
	}
}
