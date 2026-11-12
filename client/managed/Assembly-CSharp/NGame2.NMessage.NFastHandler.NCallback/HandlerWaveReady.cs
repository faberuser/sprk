using NGame2.NBattleView;
using NShared;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerWaveReady : FastHandler<CallbackMessage<WaveReady>>
{
	protected override bool OnMessage(CallbackMessage<WaveReady> message)
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			BattleViewGenericSingleton<CreatureManager>.instance.OnMessageFast(message);
		}
		return true;
	}
}
