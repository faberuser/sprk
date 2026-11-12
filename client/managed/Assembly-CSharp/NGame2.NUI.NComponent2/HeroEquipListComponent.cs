using System;
using System.Collections.Generic;
using NGame2.NDispatchBattle;
using NGame2.NUI.NHelper;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroEquipListComponent : UIScrollItemBehaviour, InitOnCreate, HeroPortraitComponent.Listener, ItemIconComponent.Listener, HeroRunehengeIcon.IDataProvider
{
	public interface Listener
	{
		void OnClick(HeroPortraitComponent component);

		void OnClick(ItemIconComponent component);

		void OnClickThis(int heroIndex);
	}

	[Serializable]
	public class EquipIconRootInfo
	{
		public EquipPartType PartType;

		public Transform IconRoot;
	}

	[Serializable]
	public class TreasureIconRootInfo
	{
		public int SlotIndex;

		public Transform IconRoot;

		public Transform SpriteLock;
	}

	public enum PanelType
	{
		Equip,
		Treasure
	}

	public const string PrefabPath = "Prefab2/Component/Icon/HeroEquipListComponent";

	public static PrefabAuto<HeroEquipListComponent> PrefabAuto = new PrefabAuto<HeroEquipListComponent>("Prefab2/Component/Icon/HeroEquipListComponent");

	public HeroRunehengeIcon RunehengeIcon;

	public Transform Root_Equip;

	public EquipIconRootInfo[] EquipIconRoot;

	public Transform Root_Treasure;

	public TreasureIconRootInfo[] TreasureIconRoot;

	public Transform HeroIconRoot;

	public UISprite SelectMark;

	public BoxCollider SelectBox;

	public UIWidget WidgetRuneTooltip;

	private RunehengeTooltipTextInfo _runeTooltipInfo;

	private Listener _listener;

	private HeroPortraitComponent _heroIconComponent;

	private List<ItemIconComponent> _equipIconList = new List<ItemIconComponent>();

	private List<ItemIconComponent> _treasureIconList = new List<ItemIconComponent>();

	private bool _selectable;

	private PanelType _currentPanelType;

	private bool _useDispatch;

	public CachedHeroInfo HeroInfo { get; private set; }

	public int HeroIndex
	{
		get
		{
			if (HeroInfo == null)
			{
				return 0;
			}
			return HeroInfo.HeroIndex;
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Clear();
	}

	public void Open(CachedHeroInfo heroInfo, Listener listener, bool selectable = false, bool useDispatch = false)
	{
		if (heroInfo != null)
		{
			SetPanel(PanelType.Equip);
			Clear();
			HeroInfo = heroInfo;
			_listener = listener;
			_selectable = selectable;
			_useDispatch = useDispatch;
			_heroIconComponent = HeroPortraitComponent.ShortPrefabAuto.Factory.Create(HeroIconRoot);
			_heroIconComponent.Open(heroInfo, this);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_heroIconComponent.transform);
			if (_useDispatch)
			{
				_heroIconComponent.SetDispatched(DispatchHelper.IsDispatchedHero(heroInfo), _heroIconComponent.PortraitMode == HeroPortraitComponent.EPortraitMode.Icon);
			}
			_runeTooltipInfo = new RunehengeTooltipTextInfo();
			_runeTooltipInfo.HeroInfo = HeroInfo;
			_runeTooltipInfo.RunePage = HeroInfo.ApplyRunePage;
			CreateEquipIcons();
			CreateTreasureIcons();
			CreateRunehengeIcon();
			if (selectable)
			{
				UIHelper.EnableGameObject(SelectBox, enable: true);
			}
		}
	}

	private void SetPanel(PanelType state)
	{
		_currentPanelType = state;
		UIHelper.EnableGameObject(Root_Equip, state == PanelType.Equip);
		UIHelper.EnableGameObject(Root_Treasure, state == PanelType.Treasure);
	}

	private void CreateRunehengeIcon()
	{
		if (!(RunehengeIcon == null))
		{
			RunehengeIcon.Open(this);
		}
	}

	private void CreateEquipIcons()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(EquipIconRoot); i++)
		{
			EquipIconRootInfo equipIconRootInfo = EquipIconRoot[i];
			if (equipIconRootInfo == null || equipIconRootInfo.IconRoot == null)
			{
				continue;
			}
			EquipItemInfo equipItemInfoByPartType = HeroInfo.GetEquipItemInfoByPartType(equipIconRootInfo.PartType);
			if (equipItemInfoByPartType == null)
			{
				if (equipIconRootInfo.PartType == EquipPartType.Treasure)
				{
					ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(equipIconRootInfo.IconRoot);
					itemIconComponent.Open(this, equipIconRootInfo.PartType, HeroInfo);
					Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
					_equipIconList.Add(itemIconComponent);
				}
			}
			else
			{
				ItemIconComponent itemIconComponent2 = ItemIconComponent.PrefabAuto.Factory.Create(equipIconRootInfo.IconRoot);
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfoByPartType.ItemIndex);
				itemIconComponent2.Open(data, equipItemInfoByPartType, this, HeroInfo);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent2.transform);
				_equipIconList.Add(itemIconComponent2);
			}
		}
	}

	private void CreateTreasureIcons()
	{
		if (__Extension_Method_ICollection__.COUNT(_treasureIconList) != 0)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TreasureIconRoot); i++)
		{
			TreasureIconRootInfo treasureIconRootInfo = TreasureIconRoot[i];
			if (treasureIconRootInfo != null && !(treasureIconRootInfo.IconRoot == null))
			{
				EquipSlotData data = GameRoot.Instance.EquipSlotDataContainer.GetData(treasureIconRootInfo.SlotIndex);
				UIHelper.EnableGameObject(treasureIconRootInfo.SpriteLock, data == null);
				EquipItemInfo equipItemInfoByPartIndex = HeroInfo.GetEquipItemInfoByPartIndex(treasureIconRootInfo.SlotIndex);
				if (equipItemInfoByPartIndex != null)
				{
					ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(treasureIconRootInfo.IconRoot);
					ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfoByPartIndex.ItemIndex);
					itemIconComponent.Open(data2, equipItemInfoByPartIndex, this, HeroInfo);
					Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
					_treasureIconList.Add(itemIconComponent);
				}
			}
		}
	}

	public void Refresh()
	{
		CachedHeroInfo heroInfo = HeroInfo;
		Listener listener = _listener;
		Clear();
		Open(heroInfo, listener, selectable: false, _useDispatch);
	}

	public void Clear()
	{
		FactoryBase.SafeReleaseComponent(ref _heroIconComponent);
		FactoryBase.ReleaseAndClearList(_equipIconList);
		FactoryBase.ReleaseAndClearList(_treasureIconList);
		UIHelper.EnableGameObject(SelectMark, enable: false);
		UIHelper.EnableGameObject(SelectBox, enable: false);
		if (RunehengeIcon != null)
		{
			RunehengeIcon.Close();
		}
		HideRuneTooltip();
	}

	public void ForEachItemComponent(Action<ItemIconComponent> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_equipIconList); i++)
		{
			if (_equipIconList[i].EquipItemInfo != null)
			{
				Vespa_System_Action_Extension_Method.fire(action, _equipIconList[i]);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_treasureIconList); j++)
		{
			Vespa_System_Action_Extension_Method.fire(action, _treasureIconList[j]);
		}
	}

	public ItemIconComponent FindEquipItem(int slotIndex)
	{
		ItemIconComponent itemIconComponent = _equipIconList.Find((ItemIconComponent e) => e.EquipItemInfo != null && e.EquipItemInfo.SlotIndex == slotIndex);
		if (itemIconComponent != null)
		{
			return itemIconComponent;
		}
		return _treasureIconList.Find((ItemIconComponent e) => e.EquipItemInfo.SlotIndex == slotIndex);
	}

	public void OnClick()
	{
		if (_selectable)
		{
			if (_listener != null)
			{
				_listener.OnClickThis(HeroInfo.HeroIndex);
			}
			Select();
		}
	}

	public void Select()
	{
		if (_selectable && !(SelectMark == null))
		{
			UIHelper.EnableGameObject(SelectMark, enable: true);
		}
	}

	public void UnSelect()
	{
		if (_selectable && !(SelectMark == null))
		{
			UIHelper.EnableGameObject(SelectMark, enable: false);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (_listener != null)
		{
			_listener.OnClick(heroPortrait);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	public void OnClickBack()
	{
		SetPanel(PanelType.Equip);
	}

	public void ShowRuneTooltip()
	{
		if (_runeTooltipInfo != null && !RuneHelper.IsEmptyRunePage(_runeTooltipInfo.HeroInfo, _runeTooltipInfo.RunePage))
		{
			RunehengeTooltip.Open(WidgetRuneTooltip, _runeTooltipInfo);
		}
	}

	private void HideRuneTooltip()
	{
		RunehengeTooltip.Hide(WidgetRuneTooltip);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (_useDispatch && DispatchHelper.IsDispatchedHero(itemIconComponent.CachedHeroInfo))
		{
			CenterMessage centerMessage = CenterMessage.Open(LocalizationManager.GetString(DispatchBattle.DISPATCH_EQUIP_FORGE_MSG));
			if (centerMessage != null)
			{
				centerMessage.SetColor(Color.green);
			}
		}
		else if (_currentPanelType == PanelType.Equip && itemIconComponent.EquipPartType == EquipPartType.Treasure)
		{
			SetPanel(PanelType.Treasure);
		}
		else if (_listener != null)
		{
			_listener.OnClick(itemIconComponent);
		}
	}

	CachedHeroInfo HeroRunehengeIcon.IDataProvider.GetHeroInfo()
	{
		return HeroInfo;
	}
}
