using NShared;

namespace NGame2.NMessage.NFastHandler.NOperation;

public class HandlerReserveOperation : FastHandler<OperationMessage<ReserveOperation>>
{
	protected override bool OnMessage(OperationMessage<ReserveOperation> message)
	{
		OnMessageByValue(message, EValueType.SkillIndex, message.FireSkillId);
		OnMessageByValue(message, EValueType.OperationUid, message.OperationUid);
		return true;
	}
}
