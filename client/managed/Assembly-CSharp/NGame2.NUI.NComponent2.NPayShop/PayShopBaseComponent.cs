using NShared;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public abstract class PayShopBaseComponent : UIScrollItemBehaviour
{
	public new interface IListener
	{
		void OnClickProduct(int index);
	}

	protected float UpdateBeginTime;

	protected IListener _listener;

	protected virtual float UpdateInterval => 0f;

	public abstract void ReleaseComponent();

	public virtual void UpdateDerived()
	{
		if (IsUpdate())
		{
			IntervalUpdateDerived();
		}
	}

	public virtual void IntervalUpdateDerived()
	{
	}

	protected bool IsUpdate()
	{
		if (UpdateInterval <= 0f)
		{
			return true;
		}
		if (UpdateBeginTime == 0f)
		{
			UpdateBeginTime = Time.time;
		}
		if (Time.time - UpdateBeginTime >= UpdateInterval)
		{
			UpdateBeginTime -= Time.time;
			return true;
		}
		return false;
	}
}
public abstract class PayShopBaseComponent<TData> : PayShopBaseComponent where TData : IDataKeySelector<int>
{
	protected TData _data;

	public int Index => _data.Key;

	public virtual void Open(TData data, IListener listener)
	{
		_listener = listener;
		_data = data;
		Init(data);
	}

	protected abstract void Init(TData data);

	public virtual void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClickProduct(_data.Key);
		}
	}

	public virtual void Refresh()
	{
		ReleaseComponent();
		Init(_data);
	}
}
