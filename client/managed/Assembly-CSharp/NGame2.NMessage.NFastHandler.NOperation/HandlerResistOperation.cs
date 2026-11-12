using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NOperation;

public class HandlerResistOperation : FastHandler<OperationMessage<ResistOperation>>
{
	protected override bool OnMessage(OperationMessage<ResistOperation> message)
	{
		OnMessageForBattleCreature(message, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}

	private void OnMessageForBattleCreature(OperationMessage<ResistOperation> message, int uid)
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
