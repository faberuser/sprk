using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NContents;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InspectEquipInfo : BaseWindow, ItemIconComponent.Listener, ClassTagIconComponent.Listener
{
	[Serializable]
	public class EntrySubMenuShortcut
	{
		public string LinkedNpcCode;

		public int EntrySubMenuIndex;

		public GameObject Button;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/InspectEquipInfo";

	[Header("Title Bar")]
	public UILabel NameLabel;

	[Space(10f)]
	[Header("Equip Info")]
	public Transform EquipIconPivot;

	public UILabel MainStatTypeLabel;

	public UILabel MainStatValueLabel;

	public UILabel MainFullyEnhancedStatLabel;

	public UISprite ExpGaugeSprite;

	public UILabel ExpPercentageLabel;

	public UISprite MaxExpGaugeSprite;

	public UILabel MaxExpPercentageLabel;

	public UIWidget UniqueWidget;

	public UILabel CreatureLimitLabel;

	public UITexture UniqueHeroTexture;

	public UILabel LevelLabel;

	public UILabel DescriptionLabel;

	public UILabel LevelLimitLabel;

	public UISimpleGrid ClassTagGrid;

	[Space(10f)]
	[Header("Add Stats")]
	public UIPanel StatPanel;

	public UIScrollView StatScrollView;

	[Space(10f)]
	[Header("Etc")]
	public UISprite Sprite_BlackBg;

	[Space(10f)]
	[Header("ShortCut")]
	public GameObject ItemLinkButtonRoot;

	public EntrySubMenuShortcut ForgeShortCut;

	public UIButton Button_Keep;

	public EntrySubMenuShortcut ValanceShortCut;

	private ItemLinkButtonComponent ItemLinkButtonComponent;

	protected ItemIconComponent _itemIconComponent;

	private StatListComponent _statListComponent;

	private List<ClassTagIconComponent> _classTagIconComponentList = new List<ClassTagIconComponent>();

	[HideInInspector]
	public IAccountInfo AccountInfo;

	public CachedHeroInfo CachedHeroInfo;

	public EquipItemInfo EquipItemInfo;

	public ItemData ItemData;

	public EquipItemData EquipItemData;

	public void Open(IAccountInfo accountInfo, CachedHeroInfo cachedHeroInfo, EquipItemInfo equipItemInfo)
	{
		Open(delegate
		{
			Init(accountInfo, cachedHeroInfo, equipItemInfo);
		}, null);
	}

	public void Open(IAccountInfo accountInfo, CachedHeroInfo cachedHeroInfo, LinkedItemInfo[] linkedItem)
	{
		if (linkedItem != null && linkedItem.Length >= 1)
		{
			Open(delegate
			{
				Init(accountInfo, cachedHeroInfo, linkedItem[0].EquipItemInfo);
			}, null);
		}
	}

	public void Init(IAccountInfo accountInfo, CachedHeroInfo cachedHeroInfo, EquipItemInfo equipItemInfo)
	{
		AccountInfo = accountInfo;
		CachedHeroInfo = cachedHeroInfo;
		EquipItemInfo = equipItemInfo;
		SetBlackBg(isEnable: false);
		InitEquipItemData();
		Refresh();
		ItemLinkButtonComponent.AddComponent(ref ItemLinkButtonRoot, ref ItemLinkButtonComponent, ItemLinkButtonComponent.EItemLinkButtonType.Link, ChatType.None);
		if (ItemLinkButtonComponent != null)
		{
			ItemLinkButtonComponent.Open(ItemLinkButtonComponent.EItemLinkButtonType.Link, new LinkedItemInfo
			{
				MaxEquipLevel = 0,
				EquipItemInfo = equipItemInfo
			});
		}
	}

	public virtual void Refresh()
	{
		if (IsOpen())
		{
			SetupInfo();
			SetupIcon();
			SetupStat();
			SetupUniqueAndClass();
			SetupMakingShortButton();
			SetupKeepButton();
		}
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		ClearStatList();
		ClearIcon();
		ClearClassTagIconList();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		if (StatScrollView != null)
		{
			StatScrollView.ResetPosition();
		}
	}

	protected override void OnShowRoot(bool isShow)
	{
		base.OnShowRoot(isShow);
	}

	public void SetBlackBg(bool isEnable)
	{
		UIHelper.EnableGameObject(Sprite_BlackBg, isEnable);
	}

	private void ClearStatList()
	{
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
	}

	private void ClearIcon()
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
	}

	private void InitEquipItemData()
	{
		if (EquipItemInfo != null)
		{
			int itemIndex = EquipItemInfo.ItemIndex;
			EquipItemData = ((ItemData<EquipItemData>)(ItemData = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemIndex)))?.SubData;
		}
	}

	public void SetupStat(EquipItemInfo otherEquipItemInfo = null)
	{
		ClearStatList();
		if (ItemData != null && EquipItemData != null && EquipItemInfo != null)
		{
			if (otherEquipItemInfo != null)
			{
				_ = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(otherEquipItemInfo.ItemIndex)?.SubData;
			}
			long statValue = 0L;
			if (EquipItemData.GetMainStat(out var statType, out statValue, EquipItemInfo.Star, EquipItemData.DetailIndex, EquipItemInfo.Level))
			{
				bool showSign = StatData.IsPercentStat(statType);
				string statDisplayValue = StatData.GetStatDisplayValue(statType, statValue, showSign);
				UIHelper.EnableGameObject(MainStatTypeLabel, enable: true);
				UIHelper.EnableGameObject(MainStatValueLabel, enable: true);
				UIHelper.SetLabelText(MainStatTypeLabel, statType);
				UIHelper.SetLabelText(MainStatValueLabel, statDisplayValue);
			}
			else
			{
				UIHelper.EnableGameObject(MainStatTypeLabel, enable: false);
				UIHelper.EnableGameObject(MainStatValueLabel, enable: false);
			}
			FactoryBase.SafeReleaseComponent(ref _statListComponent);
			_statListComponent = StatListComponent.Create(StatPanel, 5);
			StatListComponent.AddOptionStat(_statListComponent, EquipItemData, EquipItemInfo);
			StatListComponent.AddExtraOptionStat(_statListComponent, EquipItemInfo);
			StatListComponent.AddEnchantOptionStat(_statListComponent, EquipItemInfo);
			StatListComponent.AddSetOptionStat(_statListComponent, CachedHeroInfo, EquipItemData, EquipItemInfo);
			if (GameRoot.Instance.EquipItemDataContainer.IsEnhanceStone(ItemData.Code))
			{
				StatListComponent.AddDescription(_statListComponent, ItemData.Description2);
			}
			if (StatScrollView != null)
			{
				StatScrollView.ResetPosition();
			}
			VespaCoroutine.Start(this, CoInvalidatingStatListPanel());
		}
	}

	private IEnumerator CoInvalidatingStatListPanel()
	{
		yield return new WaitForEndOfFrame();
		StatPanel.Invalidate(includeChildren: true);
	}

	private void SetupInfo()
	{
		if (ItemData != null && EquipItemData != null)
		{
			UIHelper.SetLabelText(NameLabel, ItemNameBuilder.GetItemName(ItemData, EquipItemInfo));
			UIHelper.SetLabelText(DescriptionLabel, ItemData.Description);
			UIHelper.SetLabelText(LevelLimitLabel, (EquipItemData.ReqLevel <= 0) ? 1 : EquipItemData.ReqLevel);
			UIHelper.SetLabelText(LevelLabel, "");
			UIHelper.EnableGameObject(MaxExpPercentageLabel, enable: false);
			UIHelper.EnableGameObject(MaxExpGaugeSprite, enable: false);
			UIHelper.EnableGameObject(ExpPercentageLabel, enable: false);
			UIHelper.EnableGameObject(ExpGaugeSprite, enable: false);
		}
	}

	private void SetupIcon()
	{
		if (_itemIconComponent == null)
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(EquipIconPivot);
			if (_itemIconComponent == null)
			{
				return;
			}
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		_itemIconComponent.Open(ItemData, EquipItemInfo, this);
		_itemIconComponent.ApplyNotForSale(apply: true);
		_itemIconComponent.UpdateStateByHero(CachedHeroInfo);
		if (AccountInfo == null)
		{
			_itemIconComponent.SetLockSubComponent(locked: false);
		}
	}

	private void ClearClassTagIconList()
	{
		FactoryBase.ReleaseAndClearList(_classTagIconComponentList);
	}

	private void SetupClassTagIcon()
	{
		if (ClassTagGrid == null || EquipItemData == null)
		{
			return;
		}
		CreatureTagType[] tagType = EquipItemData.TagType;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(tagType))
		{
			return;
		}
		if (GameRoot.Instance.EquipItemDataContainer.ContainsAllClassTag(EquipItemData.EquipCode))
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			_classTagIconComponentList.Add(classTagIconComponent);
			classTagIconComponent.OpenAllClassTag(this);
			UIHelper.RepositionGrid(ClassTagGrid);
			return;
		}
		for (int num = tagType.Length - 1; num >= 0; num--)
		{
			CreatureTagType tagType2 = tagType[num];
			ClassTagIconComponent classTagIconComponent2 = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			if (!(classTagIconComponent2 == null))
			{
				_classTagIconComponentList.Add(classTagIconComponent2);
				classTagIconComponent2.Open(tagType2, this);
			}
		}
		UIHelper.RepositionGrid(ClassTagGrid);
	}

	private void SetupUniqueWidget(int heroIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		UIHelper.SetLabelText(CreatureLimitLabel, data.Name);
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(heroIndex, 3);
		UIHelper.ResetPlaneAssetTextureAsync(this, UniqueHeroTexture, heroIcon);
	}

	private void SetupUniqueAndClass()
	{
		ClearClassTagIconList();
		bool flag = !__Extension_Method_ICollection__.IsNullOrEmpty(EquipItemData.CreatureIndex);
		if (flag)
		{
			SetupUniqueWidget(EquipItemData.CreatureIndex[0]);
		}
		else
		{
			SetupClassTagIcon();
		}
		UIHelper.EnableGameObject(ClassTagGrid, !flag);
		UIHelper.EnableGameObject(UniqueWidget, flag);
	}

	void ClassTagIconComponent.Listener.OnClick(ClassTagIconComponent caller)
	{
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent equipIconComponent)
	{
	}

	public virtual void OnClickClose()
	{
		Close();
	}

	[UIEvent(AllowDuringPatchDownloading = false)]
	public void OnClickMakingPlace()
	{
		string empty = string.Empty;
		int num = 0;
		if (EquipItemInfo.IsTechnoMagic)
		{
			if (!SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.Valance))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("TECHNOMAGIC_ITEM_NOTICE_QUICKBUTTON"));
				return;
			}
			empty = ValanceShortCut.LinkedNpcCode;
			num = ValanceShortCut.EntrySubMenuIndex;
		}
		else
		{
			empty = ForgeShortCut.LinkedNpcCode;
			num = ForgeShortCut.EntrySubMenuIndex;
		}
		NPCData data = GameRoot.Instance.NPCDataContainer.GetData(empty);
		if (data != null)
		{
			List<object> list = new List<object>();
			list.Add(CachedHeroInfo);
			list.Add(EquipItemInfo);
			SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<ForgeShortCutWindow>().Open(data, EquipItemInfo, num, list, EquipItemInfo.IsTechnoMagic ? ForgeShortCutWindow.ShortCutType.Valance : ForgeShortCutWindow.ShortCutType.Forge);
		}
	}

	private void SetupMakingShortButton()
	{
		if (EquipItemInfo == null)
		{
			return;
		}
		if (EquipItemInfo.IsTechnoMagic)
		{
			if (!(ValanceShortCut.Button == null))
			{
				bool enable = EquipItemData.WorkPlace == EquipItemWorkPlace.Valance && EquipItemInfo.GetInventoryType() != InventoryType.Chest;
				UIHelper.EnableGameObject(ValanceShortCut.Button.GetComponentInChildren<UIButton>(), enable: true);
				UIHelper.EnableGameObject(ForgeShortCut.Button.GetComponentInChildren<UIButton>(), enable: false);
				UIHelper.EnableButton(ValanceShortCut.Button.GetComponentInChildren<UIButton>(), enable);
			}
		}
		else if (!(ForgeShortCut.Button == null))
		{
			bool enable2 = !GameRoot.Instance.EquipItemDataContainer.IsEnhanceStone(ItemData.Code) && EquipItemInfo.GetInventoryType() != InventoryType.Chest;
			UIHelper.EnableGameObject(ForgeShortCut.Button.GetComponentInChildren<UIButton>(), enable: true);
			UIHelper.EnableGameObject(ValanceShortCut.Button.GetComponentInChildren<UIButton>(), enable: false);
			UIHelper.EnableButton(ForgeShortCut.Button.GetComponentInChildren<UIButton>(), enable2);
		}
	}

	public void SetupKeepButton()
	{
		if (!(Button_Keep == null))
		{
			UIHelper.EnableGameObject(Button_Keep, enable: true);
		}
	}

	public void ChangeHero(CachedHeroInfo heroInfo)
	{
		CachedHeroInfo = heroInfo;
		Refresh();
	}
}
