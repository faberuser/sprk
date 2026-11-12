using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ShopItemFilterComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickShopItemFilter(ShopItemFilterComponent comp, IShopItemFilterInfo info);
	}

	public const string PrefabPath = "Prefab2/Component/ShopItem/ShopItemFilterComponent";

	public static PrefabAuto<ShopItemFilterComponent> PrefabAuto = new PrefabAuto<ShopItemFilterComponent>("Prefab2/Component/ShopItem/ShopItemFilterComponent");

	public GameObject GoNotice;

	public UIButton ButtonSelected;

	public UIButton ButtonNoSelected;

	public UILabel LabelName;

	public Color ColorButtonOn;

	public Color ColorButtonOff;

	private IListener _listener;

	private ShopItemSortData _shopItemInfo;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_listener = null;
		Close();
	}

	public void Open(IShopItemFilterInfo info, IListener listener)
	{
		if (info != null && info is ShopItemSortData)
		{
			_listener = listener;
			_shopItemInfo = info as ShopItemSortData;
			SetLabelLocalKey(_shopItemInfo.LocalKey);
			ShowNotice(isShow: false);
			SetSelected(isSelect: false);
		}
	}

	public void SetLabelLocalKey(string key)
	{
		string stringDirect = LocalizationManager.GetStringDirect(key);
		UIHelper.SetLabelText(LabelName, stringDirect);
	}

	public void Close()
	{
		_listener = null;
		_shopItemInfo = null;
		SetSelected(isSelect: false);
		SetEnable(isEnable: true);
		ShowNotice(isShow: false);
	}

	public void SetEnable(bool isEnable)
	{
		UIHelper.SetWidgetGrayScale(ButtonSelected, !isEnable, setColliders: true);
		UIHelper.SetWidgetGrayScale(ButtonNoSelected, !isEnable, setColliders: true);
		UIHelper.SetWidgetGrayScale(LabelName, !isEnable);
		if (!isEnable)
		{
			SetSelected(isSelect: false);
		}
	}

	public void SetSelected(bool isSelect)
	{
		UIHelper.EnableGameObject(ButtonSelected, isSelect);
		UIHelper.EnableGameObject(ButtonNoSelected, !isSelect);
		Color color = (isSelect ? ColorButtonOn : ColorButtonOff);
		UIHelper.SetLabelColor(LabelName, color);
	}

	public void OnClickFilterInfo()
	{
		if (_listener != null && _shopItemInfo != null)
		{
			_listener.OnClickShopItemFilter(this, _shopItemInfo);
		}
	}

	public IShopItemFilterInfo GetFilterInfo()
	{
		if (_shopItemInfo != null)
		{
			return _shopItemInfo;
		}
		return null;
	}

	public void ShowNotice(bool isShow)
	{
		UIHelper.EnableGameObject(GoNotice, isShow);
	}
}
