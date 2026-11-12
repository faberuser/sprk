using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroRuneItemInfo : BaseWindow, EquipRuneSlotInfoComponent.IDataProvider, IUserObserver<ItemInfo>, IUserObserver
{
	public enum EMode
	{
		Equip,
		Unequip,
		ReadOnly
	}

	public interface IListener
	{
		void OnClose(HeroRuneItemInfo caller);

		void OnEquip(HeroRuneItemInfo caller);

		void OnUnequip(HeroRuneItemInfo caller);
	}

	[Serializable]
	public class DualStatUiData
	{
		public UILabel Label_DualStatType;

		public UILabel Label_DualStatValue;
	}

	public static string PrefabPath = "Resources/Prefab2/Window/Hero/HeroRuneItemInfo";

	public static PrefabAuto<HeroRuneItemInfo> PrefabAuto = new PrefabAuto<HeroRuneItemInfo>(PrefabPath);

	public UILabel Label_ItemName;

	public UILabel Label_Description;

	public Transform ItemParent;

	[Header("[Stat]")]
	public UIWidget Widget_SingleStat;

	public UILabel Label_StatType;

	public UILabel Label_StatValue;

	[Header("[DualStat]")]
	public UIWidget Widget_DualStat;

	public List<DualStatUiData> DualStatList = new List<DualStatUiData>();

	[Header("[AddSkill]")]
	public UIWidget Widget_AddSkill;

	public UILabel Label_AddSkill;

	public UILabel SlotTypeLabel;

	public UIButton EquipButton;

	public UIButton UnequipButton;

	public UIButton CancelButton;

	public UIScrollView ScrollView;

	public EquipRuneSlotInfoComponent EquipRuneSlotInfoComponent;

	private IListener _listener;

	private ItemIconComponent _itemIconComponent;

	public ItemData ItemData { get; private set; }

	public EquipItemInfo EquipItemInfo { get; private set; }

	public PunishmentRuneOptionInfo PunishmentRuneOptionInfo
	{
		get
		{
			if (EquipItemInfo == null)
			{
				return null;
			}
			return EquipItemInfo.PunishmentRuneOption;
		}
	}

	public long ItemCount { get; private set; }

	public RuneSlotType SlotType { get; private set; }

	public EMode Mode { get; private set; }

	public void Open(IListener listener, RuneSlotType slotType, EMode infoMode, ItemData itemData, long itemCount, EquipItemInfo equipItemInfo)
	{
		base.Open(delegate
		{
			_listener = listener;
			Mode = infoMode;
			SlotType = slotType;
			ItemData = itemData;
			ItemCount = itemCount;
			EquipItemInfo = equipItemInfo;
			if (EquipRuneSlotInfoComponent != null)
			{
				EquipRuneSlotInfoComponent.Open(this);
			}
			SetItemInfo();
			SetupStat();
			SetupButtonState();
			if (ScrollView != null)
			{
				ScrollView.ResetPosition();
			}
		});
	}

	public new void Close()
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		if (_listener != null)
		{
			_listener.OnClose(this);
		}
	}

	public void SetItemInfo()
	{
		if (ItemData != null)
		{
			SetItemIcon(ItemCount);
			UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(ItemData));
			UIHelper.SetLabelText(Label_Description, ItemData.Description);
			if (EquipRuneSlotInfoComponent != null)
			{
				EquipRuneSlotInfoComponent.Refresh();
			}
		}
	}

	public void SetItemIcon(long itemCount)
	{
		ItemCount = itemCount;
		if (_itemIconComponent == null)
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ItemParent);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		}
		_itemIconComponent.Open(ItemData);
		_itemIconComponent.SetItemCount(ItemCount);
	}

	public void SetupStat()
	{
		if (ItemData != null)
		{
			SetupRuneStat();
			SetupRuneAddSkill();
		}
	}

	private void SetupRuneAddSkill()
	{
		string runeAddSkill = RuneHelper.GetRuneAddSkill(ItemData, (EquipItemInfo != null) ? EquipItemInfo.PunishmentRuneOption : null);
		UIHelper.EnableGameObject(Widget_AddSkill, !String_Extension_Method.IsNullOrEmpty(runeAddSkill));
		if (!String_Extension_Method.IsNullOrEmpty(runeAddSkill))
		{
			string runeAddSkill2 = RuneHelper.GetRuneAddSkill(ItemData, PunishmentRuneOptionInfo);
			UIHelper.SetLabelText(Label_AddSkill, runeAddSkill2);
		}
	}

	public void SetupRuneStat()
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		List<KeyValuePair<Enum, long>> list = RuneHelper.GetRuneStat(ItemData, PunishmentRuneOptionInfo).ToList();
		if (list == null)
		{
			return;
		}
		bool flag = list.Count == 1;
		UIHelper.EnableGameObject(Widget_SingleStat, flag);
		UIHelper.EnableGameObject(Widget_DualStat, !flag);
		for (int i = 0; i < list.Count; i++)
		{
			empty = LocalizationManager.GetString(list[i].Key);
			empty2 = StatData.GetStatDisplayValue(list[i].Key, list[i].Value);
			if (flag)
			{
				UIHelper.SetLabelText(Label_StatType, empty);
				UIHelper.SetLabelText(Label_StatValue, empty2);
				continue;
			}
			if (i >= DualStatList.Count)
			{
				break;
			}
			UIHelper.SetLabelText(DualStatList[i].Label_DualStatType, empty);
			UIHelper.SetLabelText(DualStatList[i].Label_DualStatValue, empty2);
		}
		RuneSlotType[] runeSlotTypes = RuneHelper.GetRuneSlotTypes(ItemData);
		string text = string.Empty;
		if (runeSlotTypes != null)
		{
			for (int j = 0; j < runeSlotTypes.Length; j++)
			{
				text += LocalizationManager.GetString(runeSlotTypes[j]);
				if (j < runeSlotTypes.Length - 1)
				{
					text += ", ";
				}
			}
		}
		UIHelper.SetLabelText(SlotTypeLabel, text);
	}

	private void SetupButtonState()
	{
		bool flag = EquipItemInfo == null || (EquipItemInfo != null && !EquipItemInfo.IsEquipped);
		UIHelper.EnableGameObject(EquipButton, Mode == EMode.Equip && flag);
		UIHelper.EnableGameObject(UnequipButton, Mode == EMode.Unequip);
		UIHelper.EnableGameObject(CancelButton, enable: false);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		if (itemInfo != null && ItemData != null && itemInfo.ItemIndex == ItemData.Index)
		{
			if (itemInfo.Count > 0)
			{
				int count = itemInfo.Count;
				SetItemIcon(count);
			}
			else
			{
				OnClickExitButton();
			}
		}
	}

	public void OnClickEquip()
	{
		if (_listener != null)
		{
			UIHelper.EnableGameObject(CancelButton, enable: true);
			UIHelper.EnableGameObject(EquipButton, enable: false);
			_listener.OnEquip(this);
		}
	}

	public void OnClickReleaseEquip()
	{
		if (_listener != null)
		{
			_listener.OnUnequip(this);
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}

	ItemData EquipRuneSlotInfoComponent.IDataProvider.GetItemData()
	{
		return ItemData;
	}
}
