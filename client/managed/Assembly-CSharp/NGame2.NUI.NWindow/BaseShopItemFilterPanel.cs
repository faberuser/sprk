using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public abstract class BaseShopItemFilterPanel<T> : ShopItemFilterGroupComponent.IListener, ShopItemFilterComponent.IListener where T : IShopItemFilterImpl, new()
{
	public GameObject Root;

	public UITable FilterTable;

	public UIScrollView FilterScrollView;

	public UITweener TweenAlpha;

	protected List<ShopItemFilterGroupComponent> _listGroupComp = new List<ShopItemFilterGroupComponent>();

	protected T _filterImpl = new T();

	public bool IsOpenFilterPanel()
	{
		return Root.activeSelf;
	}

	public void OnOffRoot(bool isOn)
	{
		if (TweenAlpha == null)
		{
			UIHelper.EnableGameObject(Root, isOn);
			if (isOn)
			{
				Reposition();
			}
		}
		else if (isOn)
		{
			OpenTween();
		}
		else
		{
			CloseTween();
		}
	}

	private void OpenTween()
	{
		UIHelper.EnableGameObject(Root, enable: true);
		Reposition();
		TweenAlpha.onFinished.Clear();
		TweenAlpha.PlayForward();
	}

	private void CloseTween()
	{
		TweenAlpha.AddOnFinished(delegate
		{
			UIHelper.EnableGameObject(Root, enable: false);
		});
		TweenAlpha.PlayReverse();
	}

	public void Reposition()
	{
		if (!(FilterScrollView == null) && !(FilterTable == null))
		{
			FilterTable.Reposition();
			FilterScrollView.ResetPosition();
		}
	}

	public virtual void Reset()
	{
		_filterImpl.Clear();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listGroupComp); i++)
		{
			ShopItemFilterGroupComponent shopItemFilterGroupComponent = _listGroupComp[i];
			if (!(shopItemFilterGroupComponent == null))
			{
				shopItemFilterGroupComponent.Reset();
			}
		}
	}

	public void ReleaseFilterComponents()
	{
		FactoryBase.ReleaseAndClearList(_listGroupComp);
	}

	public void Close()
	{
		UIHelper.EnableGameObject(Root, enable: false);
		ReleaseFilterComponents();
		_filterImpl.Clear();
		if (TweenAlpha != null)
		{
			TweenAlpha.onFinished.Clear();
		}
	}

	public abstract void SetFilterGroupComponent(EShopItemFilterGroupType filterGroupType);

	public abstract void OnClickFoldOut(bool isFoldOut);

	public abstract void OnClickShopItemFilter(ShopItemFilterComponent comp, IShopItemFilterInfo info);
}
