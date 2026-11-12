using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerFireSkill : FastHandler<CallbackMessage<FireSkill>>
{
	protected override bool OnMessage(CallbackMessage<FireSkill> message)
	{
		OnMessageForBattleCreature(message, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.SkillIndex, message.Callback.SkillId);
		return true;
	}

	private void OnMessageForBattleCreature(CallbackMessage<FireSkill> message, int uid)
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			_ = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(uid) == null;
		}
	}
}
