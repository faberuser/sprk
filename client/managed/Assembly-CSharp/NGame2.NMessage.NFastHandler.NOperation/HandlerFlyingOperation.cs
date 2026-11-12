using NShared;

namespace NGame2.NMessage.NFastHandler.NOperation;

public class HandlerFlyingOperation : FastHandler<OperationMessage<FlyingOperation>>
{
	protected override bool OnMessage(OperationMessage<FlyingOperation> message)
	{
		OnMessageByValue(message, EValueType.SkillIndex, message.FireSkillId);
		OnMessageByValue(message, EValueType.OperationUid, message.OperationUid);
		return true;
	}
}
