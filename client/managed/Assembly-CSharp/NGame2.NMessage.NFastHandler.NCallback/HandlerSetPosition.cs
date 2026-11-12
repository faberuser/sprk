using NGame2.NBattleField.NManager;
using NGame2.NBattleView;
using NGame2.NBattleView.NCreature.NHandler;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerSetPosition : FastHandler<CallbackMessage<NShared.SetPosition>>
{
	protected override bool OnMessage(CallbackMessage<NShared.SetPosition> message)
	{
		if ((message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.WaveReady || message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.DungeonChangeMove || message.Callback.MethodType == NShared.SetPosition.SetPositionMethodType.DungeonChangeMoveBackward) && SceneAutoBehaviour<WavePositionCollector>.isValidInstance)
		{
			SceneAutoBehaviour<WavePositionCollector>.instance.OnMessageFast(message);
		}
		OnMessageForBattleCreature(message, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}

	private void OnMessageForBattleCreature(CallbackMessage<NShared.SetPosition> message, int uid)
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
