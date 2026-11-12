using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerLeaveCreature : FastHandler<CallbackMessage<LeaveCreature>>
{
	protected override bool OnMessage(CallbackMessage<LeaveCreature> message)
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			BattleViewGenericSingleton<CreatureManager>.instance.OnMessageFast(message);
		}
		OnMessageByValue(message, EValueType.CreatureUid, message.Callback.CreatureUid);
		return true;
	}
}
