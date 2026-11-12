using NShared;

namespace NGame2.NMessage.NFastHandler.NOperation;

public class HandlerEndOperation : FastHandler<OperationMessage<EndOperation>>
{
	protected override bool OnMessage(OperationMessage<EndOperation> message)
	{
		OnMessageByValue(message, EValueType.SkillIndex, message.FireSkillId);
		OnMessageByValue(message, EValueType.OperationUid, message.OperationUid);
		return true;
	}
}
