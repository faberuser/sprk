using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerSpawnCreature : FastHandler<CallbackMessage<SpawnCreature>>
{
	protected override bool OnMessage(CallbackMessage<SpawnCreature> message)
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			BattleViewGenericSingleton<CreatureManager>.instance.OnMessageFast(message);
		}
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}
}
