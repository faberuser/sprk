using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerEnterCreature : FastHandler<CallbackMessage<EnterCreature>>
{
	protected override bool OnMessage(CallbackMessage<EnterCreature> message)
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			BattleViewGenericSingleton<CreatureManager>.instance.OnMessageFast(message);
		}
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}
}
