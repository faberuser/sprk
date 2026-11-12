using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerEndFireSkill : FastHandler<CallbackMessage<EndFireSkill>>
{
	protected override bool OnMessage(CallbackMessage<EndFireSkill> message)
	{
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.SkillIndex, message.Callback.SkillId);
		return true;
	}
}
