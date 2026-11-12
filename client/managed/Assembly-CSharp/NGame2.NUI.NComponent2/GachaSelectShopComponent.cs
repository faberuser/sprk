using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GachaSelectShopComponent : LinkFactoryComponent, InitOnCreate, ICustomAnimationAction
{
	public const string PrefabPath = "Prefab2/Component/PayShop2/GachaSelectShopComponent";

	public static PrefabAuto<GachaSelectShopComponent> PrefabAuto = new PrefabAuto<GachaSelectShopComponent>("Prefab2/Component/PayShop2/GachaSelectShopComponent");

	public Transform ItemRoot;

	public Animator Animator;

	public GameObject RareEffectRoot;

	private ItemIconComponent _itemIconComponent;

	private bool _isEnableRare;

	private bool _animationInit;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		_isEnableRare = false;
		_animationInit = false;
	}

	public void Open(PayShopProductItemInfo info, string itemGroupCode)
	{
		if (!(ItemRoot == null) && info != null)
		{
			ItemData itemData = GetItemData(info.ItemIndex);
			if (itemData != null && CreateItemIcon())
			{
				SetRareItem(itemGroupCode, itemData, info.ItemCount);
				_itemIconComponent.Open(itemData);
				ItemIconComponent itemIconComponent = _itemIconComponent;
				int itemCount = info.ItemCount;
				Color? color = null;
				int num = itemCount;
				itemIconComponent.SetItemCount(num, color);
			}
		}
	}

	public void Open(PayShopProductEquipItemInfo info, string itemGroupCode)
	{
		if (!(ItemRoot == null) && info != null)
		{
			ItemData itemData = GetItemData(info.ItemIndex);
			if (itemData != null && GetEquipItemData(itemData.Code) != null && CreateItemIcon())
			{
				SetRareItem(itemGroupCode, itemData, 1);
				EquipItemInfo equipItemInfo = CreateEquipItemInfo(info);
				_itemIconComponent.Open(equipItemInfo, null);
			}
		}
	}

	private bool IsRareItem(string itemGroupCode, string itemCode, int count)
	{
		return GameRoot.Instance.GachaSelectShopEffectDataContainer.IsRareItem(itemGroupCode, itemCode, count);
	}

	private void SetRareItem(string itemGroupCode, ItemData itemData, int itemCount)
	{
		if (!(RareEffectRoot == null) && !String_Extension_Method.IsNullOrEmpty(itemGroupCode) && itemData != null)
		{
			_isEnableRare = IsRareItem(itemGroupCode, itemData.Code, itemCount);
		}
	}

	private bool CreateItemIcon()
	{
		if (_itemIconComponent == null)
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ItemRoot);
			if (_itemIconComponent != null)
			{
				_itemIconComponent.ResetPosition();
			}
		}
		return _itemIconComponent != null;
	}

	private ItemData GetItemData(int itemIndex)
	{
		return GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
	}

	private EquipItemData GetEquipItemData(string itemCode)
	{
		return GameRoot.Instance.EquipItemDataContainer.GetData(itemCode);
	}

	private EquipItemInfo CreateEquipItemInfo(PayShopProductEquipItemInfo info)
	{
		return new EquipItemInfo
		{
			ItemIndex = info.ItemIndex,
			Level = info.Level,
			Star = info.Star
		};
	}

	void ICustomAnimationAction.Setup()
	{
		UIHelper.EnableGameObject(RareEffectRoot, enable: false);
	}

	void ICustomAnimationAction.Update(float delta)
	{
		if (!_animationInit)
		{
			UIHelper.EnableGameObject(RareEffectRoot, _isEnableRare);
			_animationInit = true;
		}
	}

	void ICustomAnimationAction.SetEnd()
	{
	}

	bool ICustomAnimationAction.IsEnd()
	{
		return _animationInit;
	}
}
