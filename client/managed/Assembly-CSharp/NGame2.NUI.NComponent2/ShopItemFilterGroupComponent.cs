using System;
using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ShopItemFilterGroupComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener : ShopItemFilterComponent.IListener
	{
		void OnClickFoldOut(bool isFold);
	}

	public const string PrefabPath = "Prefab2/Component/ShopItem/ShopItemFilterGroupComponent";

	public static PrefabAuto<ShopItemFilterGroupComponent> PrefabAuto = new PrefabAuto<ShopItemFilterGroupComponent>("Prefab2/Component/ShopItem/ShopItemFilterGroupComponent");

	public UIWidget Widget;

	public UILabel LabelName;

	public UISimpleGrid SimpleGrid;

	public TweenRotation Tween;

	private bool _isFoldOut;

	private Vector2 _initWidgetSize;

	private List<ShopItemFilterComponent> _listComp = new List<ShopItemFilterComponent>();

	private IListener _listener;

	void InitOnCreate.OnCreate()
	{
		if (_initWidgetSize == Vector2.zero)
		{
			_initWidgetSize = new Vector2(Widget.width, Widget.height);
		}
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(ShopItemSortGroupData groupData, IListener listener)
	{
		_listener = listener;
		_isFoldOut = true;
		SetFilter(groupData);
		SetLabel(groupData.LocalKey);
		SetTween(_isFoldOut);
	}

	public void SetFilter(ShopItemSortGroupData groupData)
	{
		IList<ShopItemSortData> dataListByGroupIndex = GameRoot.Instance.ShopItemSortDataContainer.GetDataListByGroupIndex(groupData.Index);
		SetFilter(dataListByGroupIndex);
	}

	public void SetTween(bool isForward)
	{
		if (!(Tween == null))
		{
			if (isForward)
			{
				Tween.PlayForward();
			}
			else
			{
				Tween.PlayReverse();
			}
		}
	}

	public void SetLabel(string key)
	{
		string stringDirect = LocalizationManager.GetStringDirect(key);
		UIHelper.SetLabelText(LabelName, stringDirect);
	}

	private void SetFilter<T>(IList<T> infos) where T : IShopItemFilterInfo
	{
		ReleaseComp();
		if (SimpleGrid == null)
		{
			return;
		}
		for (int i = 0; i < infos.Count; i++)
		{
			T val = infos[i];
			if (val != null)
			{
				ShopItemFilterComponent shopItemFilterComponent = UIHelper.AddGridItem(SimpleGrid, ShopItemFilterComponent.PrefabAuto);
				if (!(shopItemFilterComponent == null))
				{
					shopItemFilterComponent.Open(val, _listener);
					_listComp.Add(shopItemFilterComponent);
				}
			}
		}
		FoldOut(_isFoldOut);
		UIHelper.RepositionGrid(SimpleGrid);
	}

	public void Reset()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComp); i++)
		{
			ShopItemFilterComponent shopItemFilterComponent = _listComp[i];
			if (shopItemFilterComponent != null)
			{
				shopItemFilterComponent.SetSelected(isSelect: false);
			}
		}
	}

	public void SetEnable(bool isEnable)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComp); i++)
		{
			ShopItemFilterComponent shopItemFilterComponent = _listComp[i];
			if (shopItemFilterComponent != null)
			{
				shopItemFilterComponent.SetEnable(isEnable);
			}
		}
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_listComp);
	}

	public void Close()
	{
		_isFoldOut = true;
		ReleaseComp();
	}

	public void FoldOut(bool isFoldOut)
	{
		if (!(Widget == null) && !(SimpleGrid == null))
		{
			if (isFoldOut)
			{
				int num = Mathf.CeilToInt((float)__Extension_Method_ICollection__.COUNT(_listComp) / (float)SimpleGrid.Grid.maxPerLine);
				Widget.height = (int)_initWidgetSize.y + num * (int)SimpleGrid.Grid.cellHeight;
			}
			else
			{
				Widget.height = (int)_initWidgetSize.y;
			}
			NGUITools.UpdateWidgetCollider(Widget.gameObject);
			UIHelper.EnableGameObject(SimpleGrid, isFoldOut);
		}
	}

	public void OnClickFoldOut()
	{
		if (_listener != null)
		{
			_isFoldOut = !_isFoldOut;
			FoldOut(_isFoldOut);
			SetTween(_isFoldOut);
			_listener.OnClickFoldOut(_isFoldOut);
		}
	}

	public void ForFilterComponent(Action<ShopItemFilterComponent> doAction)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComp); i++)
		{
			ShopItemFilterComponent shopItemFilterComponent = _listComp[i];
			if (!(shopItemFilterComponent == null))
			{
				doAction(shopItemFilterComponent);
			}
		}
	}
}
