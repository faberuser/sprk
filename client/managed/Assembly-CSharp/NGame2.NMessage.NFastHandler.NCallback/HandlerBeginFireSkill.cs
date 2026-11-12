using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerBeginFireSkill : FastHandler<CallbackMessage<BeginFireSkill>>
{
	protected override bool OnMessage(CallbackMessage<BeginFireSkill> message)
	{
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		OnMessageByValue(message, EValueType.SkillIndex, message.Callback.SkillId);
		return true;
	}
}
