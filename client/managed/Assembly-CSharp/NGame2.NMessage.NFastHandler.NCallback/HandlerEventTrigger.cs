using System;
using NGame2.NEventTrigger;
using NShared;
using NVespa;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerEventTrigger : FastHandler<CallbackMessage<EventTrigger>>
{
	protected override bool OnMessage(CallbackMessage<EventTrigger> message)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(message.Callback.Values))
		{
			return false;
		}
		if (string.Equals(message.Callback.Values[0], "EventTrigger", StringComparison.OrdinalIgnoreCase))
		{
			OnEventTrigger_EventTrigger(message.Callback);
		}
		return true;
	}

	private void OnEventTrigger_EventTrigger(EventTrigger callback)
	{
		if (__Extension_Method_ICollection__.COUNT(callback.Values) >= 2 && String_Extension_Method.TryToInt(callback.Values[1], out var output))
		{
			SingletonAuto<EventTriggerManager>.instance.RaiseEventDirect(output);
		}
	}
}
