using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerSpawnProjectile : FastHandler<CallbackMessage<SpawnProjectile>>
{
	protected override bool OnMessage(CallbackMessage<SpawnProjectile> message)
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			BattleViewGenericSingleton<CreatureManager>.instance.OnMessageFast(message);
		}
		return true;
	}
}
