using NShared;

namespace NGame2.NMessage.NFastHandler.NOperation;

public class HandlerBeginOperation : FastHandler<OperationMessage<BeginOperation>>
{
	protected override bool OnMessage(OperationMessage<BeginOperation> message)
	{
		OnMessageByValue(message, EValueType.SkillIndex, message.FireSkillId);
		OnMessageByValue(message, EValueType.OperationUid, message.OperationUid);
		return true;
	}
}
