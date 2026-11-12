using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroPresetStorageComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Hero/HeroPresetListComponent";

	public static PrefabAuto<HeroPresetStorageComponent> PrefabAuto = new PrefabAuto<HeroPresetStorageComponent>("Resources/Prefab2/Component/Hero/HeroPresetListComponent");

	public Transform TransHeroPortraitRoot;

	public UISimpleGrid UISimpleGrid;

	public UILabel LabelSkillPage;

	public UILabel LabelRunePage;

	public EquipPartType[] equipParts;

	public UIWidget WidgetTranscnedTooltip;

	public UIWidget WidgetRunePageTooltip;

	private HeroPortraitComponent _heroPortraitComponent;

	private List<ItemIconComponent> _listEquipItemComponent = new List<ItemIconComponent>();

	private TranscendSkillPageTooltipInfo _tooltipInfo;

	private RunehengeTooltipTextInfo _runeTooltipInfo;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		_tooltipInfo = null;
		ReleaseHeroPortrait();
		ReleaseEquipItems();
	}

	public void Open(HeroPresetInfo heroPresetInfo)
	{
		if (heroPresetInfo != null)
		{
			SetHeroProtrait(heroPresetInfo.HeroIndex);
			SetRunePage(heroPresetInfo.ApplyRunePage);
			SetSkiiPage(heroPresetInfo.SkillPage);
			SetEquipItems(heroPresetInfo);
			_tooltipInfo = new TranscendSkillPageTooltipInfo
			{
				HeroIndex = heroPresetInfo.HeroIndex,
				TranscnedSkillPage = heroPresetInfo.SkillPage
			};
			_runeTooltipInfo = new RunehengeTooltipTextInfo
			{
				HeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroPresetInfo.HeroIndex),
				RunePage = heroPresetInfo.ApplyRunePage
			};
		}
	}

	private void SetEquipItems(HeroPresetInfo heroPresetInfo)
	{
		ReleaseEquipItems();
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroPresetInfo.HeroIndex);
		if (heroInfo == null)
		{
			return;
		}
		CachedHeroInfo cachedHeroInfo = JsonMarshaler.Clone(heroInfo);
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			string equipItemUid = heroPresetInfo.GetEquipItemUid(allEquipSlotDatum.Index);
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemInfoByUid(equipItemUid);
			if (equipItemInfo != null && equipItemInfo.GetInventoryType() == InventoryType.Chest)
			{
				equipItemInfo = null;
			}
			int equipItemSlotIndex = equipItemInfo?.SlotIndex ?? 0;
			cachedHeroInfo.SetEquipItemSlotIndex(allEquipSlotDatum.Index, equipItemSlotIndex);
			cachedHeroInfo.ChangeEquipItemInfo(allEquipSlotDatum.Index, equipItemInfo);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(equipParts); i++)
		{
			EquipPartType partType = equipParts[i];
			EquipItemInfo equipItemInfoByPartType = cachedHeroInfo.GetEquipItemInfoByPartType(partType);
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(UISimpleGrid, ItemIconComponent.PrefabAuto);
			if (equipItemInfoByPartType == null)
			{
				itemIconComponent.Open(null, partType, cachedHeroInfo);
			}
			else
			{
				itemIconComponent.Open(equipItemInfoByPartType, null, cachedHeroInfo);
			}
			_listEquipItemComponent.Add(itemIconComponent);
		}
		UIHelper.RepositionGrid(UISimpleGrid);
	}

	private void ReleaseEquipItems()
	{
		FactoryBase.ReleaseAndClearList(_listEquipItemComponent);
	}

	private void SetRunePage(int runePage)
	{
		UIHelper.SetLabelText(LabelRunePage, (runePage == 0) ? RuneHelper.GetDefaultRunePage() : runePage);
	}

	private void SetSkiiPage(int skillPage)
	{
		UIHelper.SetLabelText(LabelSkillPage, skillPage);
	}

	private void SetHeroProtrait(int heroIndex)
	{
		ReleaseHeroPortrait();
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		if (heroInfo != null && !(TransHeroPortraitRoot == null))
		{
			HeroPortraitComponent heroPortraitComponent = HeroPortraitComponent.SmallPrefabAuto.Factory.Create(TransHeroPortraitRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroPortraitComponent.transform);
			heroPortraitComponent.Open(heroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
			_heroPortraitComponent = heroPortraitComponent;
		}
	}

	private void ReleaseHeroPortrait()
	{
		FactoryBase.SafeReleaseComponent(ref _heroPortraitComponent);
	}

	public void ShowTooltip()
	{
		if (_tooltipInfo != null && _tooltipInfo.HeroIndex != 0 && _tooltipInfo.TranscnedSkillPage != 0)
		{
			TranscendSkillPageTooltip.Open(WidgetTranscnedTooltip, _tooltipInfo);
		}
	}

	public void HideTooltip()
	{
		TranscendSkillPageTooltip.Hide(WidgetTranscnedTooltip);
	}

	public void ShowRunePageTooltip()
	{
		if (_runeTooltipInfo != null && !RuneHelper.IsEmptyRunePage(_runeTooltipInfo.HeroInfo, _runeTooltipInfo.RunePage))
		{
			RunehengeTooltip.Open(WidgetRunePageTooltip, _runeTooltipInfo);
		}
	}

	public void HideRunePageTooltip()
	{
		RunehengeTooltip.Hide(WidgetRunePageTooltip);
	}
}
