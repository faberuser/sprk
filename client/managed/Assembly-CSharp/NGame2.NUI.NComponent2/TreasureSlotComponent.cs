using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class TreasureSlotComponent : UIScrollItemBehaviour, ItemIconComponent.Listener, InitOnCreate
{
	public interface Listener
	{
		void OnClick(TreasureSlotComponent caller);
	}

	public static string PrefabPath = "Prefab2/Component/Icon/TreasureSlotComponent";

	public static PrefabAuto<TreasureSlotComponent> PrefabAuto = new PrefabAuto<TreasureSlotComponent>(PrefabPath);

	public Transform Root_Disable;

	public Transform Root_Select;

	public Transform Root_ItemIcon;

	public Transform Root_Empty;

	public bool isSelectableSlot;

	private ItemIconComponent _itemIconComponent;

	private CachedHeroInfo _cachedHeroInfo;

	private Listener _listener;

	public int EquipSlotIndex
	{
		get
		{
			if (SlotInfo == null)
			{
				return 0;
			}
			return SlotInfo.EquipSlotIndex;
		}
	}

	public EquipItemInfo EquipItemInfo
	{
		get
		{
			if (SlotInfo == null)
			{
				return null;
			}
			return SlotInfo.EquipItemInfo;
		}
	}

	public EquipSlotInfo SlotInfo { get; private set; }

	public ItemIconComponent ItemIconComponent => _itemIconComponent;

	void InitOnCreate.OnCreate()
	{
		Reset();
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}

	private void Reset()
	{
		SlotInfo = null;
		_cachedHeroInfo = null;
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		ResetUI();
	}

	public void Open(Listener listener, EquipSlotInfo equipSlotinfo, CachedHeroInfo cachedHeroInfo)
	{
		_listener = listener;
		SlotInfo = equipSlotinfo;
		_cachedHeroInfo = cachedHeroInfo;
		Init();
	}

	public void Open(Listener listener, int equipSlotIndex, EquipItemInfo equipItemInfo, CachedHeroInfo cachedHeroInfo)
	{
		Open(listener, new EquipSlotInfo(equipSlotIndex, equipItemInfo), cachedHeroInfo);
	}

	private void Init()
	{
		UIHelper.EnableGameObject(Root_Empty, !IsEquippedSlot());
		UIHelper.EnableGameObject(Root_ItemIcon, IsEquippedSlot());
		if (IsEquippedSlot())
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(Root_ItemIcon);
			_itemIconComponent.Open(SlotInfo.EquipItemInfo, this, _cachedHeroInfo);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		}
	}

	public bool IsValidSlot()
	{
		if (SlotInfo != null)
		{
			return SlotInfo.EquipSlotIndex != 0;
		}
		return false;
	}

	public bool IsEquippedSlot()
	{
		if (SlotInfo != null)
		{
			return SlotInfo.EquipItemInfo != null;
		}
		return false;
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void SetEquipState(EquipItemInfo toEquipItemInfo)
	{
		isSelectableSlot = IsValidSlot(toEquipItemInfo);
		UIHelper.EnableGameObject(Root_Select, isSelectableSlot);
	}

	private bool IsValidSlot(EquipItemInfo equipItemInfo)
	{
		if (SlotInfo == null)
		{
			return false;
		}
		return EquipSlotHelper.IsValidSlot(SlotInfo.EquipSlotIndex, equipItemInfo);
	}

	public void SetSwapState(int targetSlotIndex)
	{
		isSelectableSlot = IsSwapAbleSlot(targetSlotIndex);
		UIHelper.EnableGameObject(Root_Disable, !isSelectableSlot);
		UIHelper.EnableGameObject(Root_Select, isSelectableSlot);
	}

	public bool IsSwapAbleSlot(int targetSlotIndex)
	{
		if (SlotInfo == null)
		{
			return false;
		}
		if (SlotInfo.EquipItemInfo == null)
		{
			return false;
		}
		if (SlotInfo.EquipSlotIndex != 0 && SlotInfo.EquipSlotIndex == targetSlotIndex)
		{
			return false;
		}
		return EquipSlotHelper.IsValidSlot(targetSlotIndex, SlotInfo.EquipItemInfo);
	}

	public void ResetUI()
	{
		UIHelper.EnableGameObject(Root_Disable, enable: false);
		UIHelper.EnableGameObject(Root_Select, enable: false);
	}

	public void AdjustToParentWidgetScale()
	{
		if (base.transform.parent == null)
		{
			return;
		}
		UIWidget component = base.transform.parent.GetComponent<UIWidget>();
		if (!(component == null))
		{
			UIWidget component2 = base.transform.GetComponent<UIWidget>();
			if (!(component2 == null))
			{
				Vector3 localScale = new Vector2((float)component.width / (float)Mathf.Max(1, component2.width), (float)component.height / (float)Mathf.Max(1, component2.height));
				localScale.z = Mathf.Max(localScale.x, localScale.y);
				base.transform.localScale = localScale;
			}
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		OnClick();
	}
}
