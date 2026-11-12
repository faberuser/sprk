using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerHeal : FastHandler<CallbackMessage<Heal>>
{
	protected override bool OnMessage(CallbackMessage<Heal> message)
	{
		OnMessageForBattleCreature(message, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}

	private void OnMessageForBattleCreature(CallbackMessage<Heal> message, int uid)
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(uid);
			if (!(creature == null))
			{
				creature.OnMessageFast(message);
			}
		}
	}
}
