using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public abstract class PayShopBaseTabComponent<TCategory> : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClickTab(TCategory category);
	}

	public GameObject Selected;

	public GameObject Unselected;

	protected IListener _listener;

	private TCategory _category;

	public virtual void Open(IListener listener, TCategory category)
	{
		_listener = listener;
		_category = category;
	}

	public virtual void OnClickTab()
	{
		if (_listener != null)
		{
			_listener.OnClickTab(_category);
		}
	}

	public void Select(bool isSelect)
	{
		UIHelper.EnableGameObject(Selected, isSelect);
		UIHelper.EnableGameObject(Unselected, !isSelect);
	}
}
