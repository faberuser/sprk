using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerDamage : FastHandler<CallbackMessage<Damage>>
{
	protected override bool OnMessage(CallbackMessage<Damage> message)
	{
		OnMessageForBattleCreature(message, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}

	private void OnMessageForBattleCreature(CallbackMessage<Damage> message, int uid)
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
