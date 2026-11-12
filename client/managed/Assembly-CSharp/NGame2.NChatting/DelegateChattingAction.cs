using System;
using NShared;

namespace NGame2.NChatting;

public class DelegateChattingAction<T> : IChattingAction
{
	public Func<T, string> Converter;

	public Action<ChattingActionData, T> Handler;

	string IChattingAction.GetName()
	{
		return typeof(T).Name;
	}

	string IChattingAction.ConvertChat(ChattingActionData data)
	{
		T val = JsonMarshaler.Parse<T>(data.Data);
		if (val == null)
		{
			return "Unknown";
		}
		return Vespa_System_Action_Extension_Method.fire(Converter, val);
	}

	void IChattingAction.Run(ChattingActionData data)
	{
		T i_argB = JsonMarshaler.Parse<T>(data.Data);
		Vespa_System_Action_Extension_Method.fire(Handler, data, i_argB);
	}
}
