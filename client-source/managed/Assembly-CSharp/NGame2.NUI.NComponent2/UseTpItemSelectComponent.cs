using System;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class UseTpItemSelectComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickComponent(int itemIndex);

		void ChangeHeroTpInfo(int transcendPoint);
	}

	public const string PrefabPath = "Prefab2/Component/Hero/UseTpItemSelectComponent";

	public static PrefabAuto<UseTpItemSelectComponent> PrefabAuto = new PrefabAuto<UseTpItemSelectComponent>("Prefab2/Component/Hero/UseTpItemSelectComponent");

	public GameObject GameObjSelect;

	public Transform TransformTpItemRoot;

	public UILabel LabelTpItemName;

	public UILabel LabelTpCount;

	public UIButton ButtonPlus;

	public UIButton ButtonMinus;

	public bool IsSelect;

	private ItemInfo _itemInfo;

	private CachedHeroInfo _heroInfo;

	private ItemIconComponent _tpItemComponent;

	private IListener _listener;

	public void Open(CachedHeroInfo heroInfo, ItemInfo itemInfo, IListener listener)
	{
		_heroInfo = heroInfo;
		_itemInfo = itemInfo;
		_listener = listener;
		SetItemComponent();
		SelectComponent(isSelect: false);
	}

	private void SetItemComponent()
	{
		ItemData itemData = GetItemData();
		if (itemData != null)
		{
			_tpItemComponent = ItemIconComponent.PrefabAuto.Factory.Create(TransformTpItemRoot);
			if (!(_tpItemComponent == null))
			{
				_tpItemComponent.ResetPosition();
				_tpItemComponent.Open(itemData);
				ItemIconComponent tpItemComponent = _tpItemComponent;
				int itemCount = GetItemCount();
				Color? color = null;
				int num = itemCount;
				tpItemComponent.SetItemCount(num, color);
				UIHelper.SetLabelText(LabelTpItemName, itemData.Name);
			}
		}
	}

	public void SelectComponent(int itemIndex)
	{
		if (_itemInfo == null)
		{
			SelectComponent(isSelect: false);
		}
		SelectComponent(_itemInfo.ItemIndex == itemIndex);
	}

	public void SelectComponent(bool isSelect)
	{
		IsSelect = isSelect;
		UIHelper.EnableGameObject(GameObjSelect, isSelect);
		if (!isSelect)
		{
			SetLabelTpCount(0);
		}
	}

	public void GetUseTpItemInfo(out int itemIndex, out int useTpCount, out int useTpValue, out int overPoint)
	{
		ItemData itemData = GetItemData();
		if (itemData == null)
		{
			itemIndex = 0;
			useTpCount = 0;
			overPoint = 0;
			useTpValue = 0;
			return;
		}
		itemIndex = _itemInfo.ItemIndex;
		useTpCount = GetLabelTpCount();
		short transcendPoint = _heroInfo.TranscendPoint;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxExtraTranscendPoint");
		int valueInt2 = itemData.GetValueInt(0);
		int num = valueInt - transcendPoint - valueInt2 * useTpCount;
		useTpValue = ((num < 0) ? (valueInt2 * useTpCount + num) : (valueInt2 * useTpCount));
		overPoint = ((num < 0) ? Math.Abs(num) : 0);
	}

	public void OnClickComponent()
	{
		if (!IsSelect && _listener != null && _itemInfo != null)
		{
			_listener.OnClickComponent(_itemInfo.ItemIndex);
		}
	}

	public void OnClickPlus()
	{
		OnClickComponent();
		int num = GetLabelTpCount() + 1;
		if (GetUseableTpCount(num) == num)
		{
			SetLabelTpCount(num);
			ChangeHeroTpInfo(num);
		}
	}

	public void OnClickMinus()
	{
		OnClickComponent();
		int num = GetLabelTpCount() - 1;
		if (GetUseableTpCount(num) == num)
		{
			SetLabelTpCount(num);
			ChangeHeroTpInfo(num);
		}
	}

	private int GetLabelTpCount()
	{
		int result = 0;
		if (String_Extension_Method.IsNullOrEmpty(LabelTpCount.text))
		{
			return result;
		}
		int.TryParse(LabelTpCount.text, out result);
		return result;
	}

	private void SetLabelTpCount(int count)
	{
		UIHelper.SetLabelText(LabelTpCount, count);
	}

	private void ChangeHeroTpInfo(int tpCount)
	{
		if (_listener != null)
		{
			ItemData itemData = GetItemData();
			if (itemData != null)
			{
				short transcendPoint = _heroInfo.TranscendPoint;
				int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxExtraTranscendPoint");
				int val = itemData.GetValueInt(0) * tpCount;
				int val2 = valueInt - transcendPoint;
				int transcendPoint2 = Math.Min(val, val2);
				_listener.ChangeHeroTpInfo(transcendPoint2);
			}
		}
	}

	public bool IsValidItemCount()
	{
		int labelTpCount = GetLabelTpCount();
		int useableTpCount = GetUseableTpCount(labelTpCount);
		if (labelTpCount == useableTpCount)
		{
			return labelTpCount > 0;
		}
		return false;
	}

	private bool CheckUseItemCount(int count)
	{
		if (count < 0)
		{
			return false;
		}
		ItemData itemData = GetItemData();
		if (itemData == null)
		{
			return false;
		}
		short transcendPoint = _heroInfo.TranscendPoint;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxExtraTranscendPoint");
		if (transcendPoint >= valueInt)
		{
			return false;
		}
		int valueInt2 = itemData.GetValueInt(0);
		if ((int)Math.Ceiling((float)(valueInt - transcendPoint) / (float)valueInt2) < count)
		{
			return false;
		}
		return true;
	}

	private int GetUseableTpCount(int useCount)
	{
		if (useCount < 0)
		{
			return 0;
		}
		ItemData itemData = GetItemData();
		if (itemData == null)
		{
			return 0;
		}
		short transcendPoint = _heroInfo.TranscendPoint;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxExtraTranscendPoint");
		if (transcendPoint >= valueInt)
		{
			return 0;
		}
		int valueInt2 = itemData.GetValueInt(0);
		int val = (int)Math.Ceiling((float)(valueInt - transcendPoint) / (float)valueInt2);
		int itemCount = GetItemCount();
		return Math.Min(Math.Min(val, itemCount), useCount);
	}

	private ItemData GetItemData()
	{
		if (_itemInfo == null)
		{
			return null;
		}
		return GameRoot.Instance.ItemDataContainer.GetData(_itemInfo.ItemIndex);
	}

	private int GetItemCount()
	{
		if (_itemInfo == null)
		{
			return 0;
		}
		return _itemInfo.Count;
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		if (_tpItemComponent != null)
		{
			FactoryBase.ReleaseComponent(_tpItemComponent);
			_tpItemComponent = null;
		}
		SelectComponent(isSelect: false);
	}
}
