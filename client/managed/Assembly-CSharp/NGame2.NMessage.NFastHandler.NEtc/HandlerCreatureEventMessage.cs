using NGame2.NBattleView;

namespace NGame2.NMessage.NFastHandler.NEtc;

public class HandlerCreatureEventMessage : FastHandler<CreatureEventMessage>
{
	protected override bool OnMessage(CreatureEventMessage message)
	{
		OnMessageForBattleCreature(message, message.CreatureUid);
		OnMessageByValue(message, EValueType.CreatureUid, message.CreatureUid);
		return true;
	}

	private void OnMessageForBattleCreature(CreatureEventMessage message, int uid)
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
