using NGame2.NBattleView;
using NGame2.NBattleView.NCreature.NHandler;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerChangeState : FastHandler<CallbackMessage<ChangeState>>
{
	protected override bool OnMessage(CallbackMessage<ChangeState> message)
	{
		OnMessageForBattleCreature(message, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}

	private void OnMessageForBattleCreature(CallbackMessage<ChangeState> message, int uid)
	{
		if (!BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			return;
		}
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(uid);
		if (!(creature == null))
		{
			if (creature.NodeTreeController.ExternalHandler is CrowdControl crowdControl)
			{
				crowdControl.OnMessageFast(message);
			}
			creature.OnMessageFast(message);
		}
	}
}
