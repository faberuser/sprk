using NGame2;
using NGame2.NMessage;
using NShared;

public static class Game2_IMessage_ExtensionMethod
{
	public static T CastMessage<T>(this IMessage This) where T : class, IMessage
	{
		return This as T;
	}

	public static CallbackMessage<TBattleCallback> CastCallback<TBattleCallback>(this IMessage This) where TBattleCallback : BaseBattleCallback
	{
		return This.CastMessage<CallbackMessage<TBattleCallback>>();
	}

	public static OperationMessage<TBattleCallback> CastOperation<TBattleCallback>(this IMessage This) where TBattleCallback : BaseBattleCallback
	{
		return This.CastMessage<OperationMessage<TBattleCallback>>();
	}

	public static bool IsValidMessage(this IMessage This)
	{
		if (This != null)
		{
			return !This.IsDummy;
		}
		return false;
	}
}
