using NShared;

namespace NGame2.NMessage.NFastHandler.NOperation;

public class HandlerCancelOperation : FastHandler<OperationMessage<CancelOperation>>
{
	protected override bool OnMessage(OperationMessage<CancelOperation> message)
	{
		OnMessageByValue(message, EValueType.SkillIndex, message.FireSkillId);
		OnMessageByValue(message, EValueType.OperationUid, message.OperationUid);
		return true;
	}
}
