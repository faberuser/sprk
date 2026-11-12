using NShared;
using NVespa;

namespace NGame2.NMessage;

public class CallbackMessage<TBattleCallback> : SmallPooling<CallbackMessage<TBattleCallback>>, ISmallPoolingItem, IMessage where TBattleCallback : BaseBattleCallback
{
	public bool IsDummy { get; set; }

	public bool IsUsed { get; private set; }

	public TBattleCallback Callback { get; private set; }

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
		IsUsed = false;
	}

	void ISmallPoolingItem.OnRelease()
	{
	}

	public void Use()
	{
		IsUsed = true;
	}

	void IMessage.ReleaseSelf()
	{
		SmallPooling<CallbackMessage<TBattleCallback>>.ReleaseInstance(this);
	}

	public void Open(TBattleCallback callback)
	{
		Callback = callback;
	}
}
