using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerBeginMove : FastHandler<CallbackMessage<BeginMove>>
{
	protected override bool OnMessage(CallbackMessage<BeginMove> message)
	{
		OnMessageForBattleCreature(message, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}

	private void OnMessageForBattleCreature(CallbackMessage<BeginMove> message, int uid)
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
