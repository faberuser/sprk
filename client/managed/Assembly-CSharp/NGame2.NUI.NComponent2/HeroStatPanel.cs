using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroStatPanel : MonoBehaviour, IHeroPanel, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<ItemInfo>, IUserObserver<NGame2.NAccount.UserSoulWeaponManager.SoulWeaponChangedInfo>
{
	public interface Listener
	{
		bool OnCloseRequest(HeroStatPanel closeThis);

		void OnClickShortCut(CachedHeroInfo heroInfo, AwakeStateType awakeState);

		void OnClickTranscend(CachedHeroInfo heroInfo, AwakeStateType awakeState);

		void OnClickLimitBreakLevelUp(CachedHeroInfo heroInfo);

		void OnClickLimitBreakExpUp(CachedHeroInfo heroInfo);

		void OnClickCraftItem(ItemIconComponent caller);
	}

	public enum AwakeStateType
	{
		None,
		NotEnoughMaterial,
		EnoughMaterial,
		InDungeon,
		Purifying,
		FullyAwakened,
		NotEnoughLevel
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/HeroStatPanel";

	public static PrefabAuto<HeroStatPanel> PrefabAuto = new PrefabAuto<HeroStatPanel>("Resources/Prefab2/Component/Hero/HeroStatPanel");

	public UIScrollView StatScroll;

	public UILabel AwakeStatusDesc;

	public UILabel AwakeStatusShort;

	public UIButton ShortCutButton;

	public UIButton TranscendButton;

	public UIButton LimitBreakLevelUpButton;

	public UIButton LimitBreakExpUpButton;

	public UITable HeroStatTable;

	public UIResizeByGrid HeroAttributeResizeGrid;

	public UISimpleGrid HeroAttributeGrid;

	public UIResizeByGrid StatResizeGrid;

	public UISimpleGrid StatGrid;

	public UIResizeByGrid ExtraOptionResizeGrid;

	public UISimpleGrid ExtraOptionGrid;

	public UIWidget ExtraOption;

	public UISimpleGrid AwakeGrid;

	public UISimpleGrid LimitBreakMaterialGrid;

	public Transform TranscendModeRoot;

	public Transform AwakenTypeRoot;

	public Transform LimitBreakLevelModeRoot;

	public Transform LimitBreakExpModeRoot;

	public CachedHeroInfo CachedHeroInfo;

	public UIWidget WidgetAwakeTranscendInfo;

	public UIWidget WidgetLimitBreakInfo;

	public UIWidget WidgetCannotTranscendAnyMore;

	public UILabel Label_TranscendLock;

	public UILabel Label_BaseInfo;

	public float HeroAttributeExtraSize_Y;

	public float HeroAttributeGrid_Y;

	public UIWidget RootWidget;

	public UILabel Label_EnhanceButton;

	public UILabel Label_AwakeButton;

	public UIClipSprite Sprite_LimitBreakExpGauge;

	public UILabel Label_LimitBreakExpGauge;

	private List<LabelFieldComponent> _attributeList = new List<LabelFieldComponent>();

	private List<LabelFieldComponent> _baseStatList = new List<LabelFieldComponent>();

	private List<LabelFieldComponent> _optionStatList = new List<LabelFieldComponent>();

	private List<HeroAwakeItemComponent> _awakeMaterialList = new List<HeroAwakeItemComponent>();

	private HeroAwakeItemComponent _transcendMaterial;

	private Listener _listener;

	private HeroAwakeType awakeType;

	private AwakeStateType awakeState;

	private bool _isMine;

	public void Open(CachedHeroInfo cachedHeroInfo, Listener listener, bool isMine = true)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		CachedHeroInfo = cachedHeroInfo;
		_listener = listener;
		_isMine = isMine;
		SetupAwakeMaterial();
		SetupAttribute();
		SetupStat();
		SetupOptions();
		ReserveRepositionHeroStatTable();
		EnterHeroStatPage triggerEvent = new EnterHeroStatPage();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	private void CalcBounds()
	{
		Bounds bounds = default(Bounds);
		List<Transform> childList = HeroStatTable.GetChildList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(childList); i++)
		{
			Transform content = childList[i];
			Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(RootWidget.transform, content, considerInactive: true);
			bounds.Encapsulate(bounds2);
		}
		RootWidget.width = (int)bounds.size.x;
		RootWidget.height = (int)bounds.size.y;
		RootWidget.ForceUpdateAnchors(includeChildren: true);
	}

	private void ReserveRepositionHeroStatTable()
	{
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			if ((bool)HeroStatTable)
			{
				CalcBounds();
				HeroStatTable.Reposition();
				HeroStatTable.repositionNow = true;
			}
		});
	}

	private void UpdateScroll()
	{
		VespaCoroutine.Start(this, UpdateScroll(StatScroll.verticalScrollBar.value));
	}

	private IEnumerator UpdateScroll(float scroll)
	{
		yield return null;
		if (!ExtraOption.gameObject.activeSelf)
		{
			StatScroll.ResetPosition();
			yield break;
		}
		StatScroll.verticalScrollBar.value = scroll;
		StatScroll.UpdatePosition();
	}

	public void ReserveCache(UiPreloader preloader)
	{
	}

	public void Reset()
	{
		_listener = null;
		FactoryBase.ReleaseAndClearList(_attributeList);
		ClearAwakeMaterial();
		ClearStat();
		ClearOption();
	}

	public void ResetScroll()
	{
		if (StatScroll != null)
		{
			SingletonUpdater<Executer>.instance.NextRun(delegate
			{
				StatScroll.ResetPosition();
			});
		}
	}

	private void ResetAndUpdateScroll()
	{
		StatScroll.ResetPosition();
		StatScroll.UpdatePosition();
	}

	private void ClearStat()
	{
		FactoryBase.ReleaseAndClearList(_baseStatList);
	}

	private void ClearAwakeMaterial()
	{
		FactoryBase.ReleaseAndClearList(_awakeMaterialList);
		FactoryBase.SafeReleaseComponent(ref _transcendMaterial);
	}

	private void ClearOption()
	{
		FactoryBase.ReleaseAndClearList(_optionStatList);
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		Reset();
	}

	private void SetupAttribute()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(CachedHeroInfo.HeroIndex);
		CreatureTagType creatureTagType = data?.TagType ?? CreatureTagType.None;
		LabelFieldComponent labelFieldComponent = UIHelper.AddGridItem(HeroAttributeGrid, LabelFieldComponent.PrefabAuto);
		labelFieldComponent.Open(LocalizationManager.GetString(HeroClass.CLASS_ATTRIBUTE), LocalizationManager.GetString(creatureTagType));
		_attributeList.Add(labelFieldComponent);
		SkillAttrType skillAttrType = ((data != null && data.AttrType != null) ? data.AttrType[0] : SkillAttrType.None);
		LabelIconFieldComponent labelIconFieldComponent = UIHelper.AddGridItem(HeroAttributeGrid, LabelIconFieldComponent.PrefabAuto);
		labelIconFieldComponent.Open(LocalizationManager.GetString(HeroAttribute.ATTACK_ATTRIBUTE), LocalizationManager.GetString(skillAttrType), skillAttrType);
		_attributeList.Add(labelIconFieldComponent);
		CreaturePositionType creaturePositionType = data?.PositionType ?? CreaturePositionType.Forward;
		LabelFieldComponent labelFieldComponent2 = UIHelper.AddGridItem(HeroAttributeGrid, LabelFieldComponent.PrefabAuto);
		labelFieldComponent2.Open(LocalizationManager.GetString(HeroAttribute.BATTLE_POSITION), LocalizationManager.GetString(creaturePositionType));
		_attributeList.Add(labelFieldComponent2);
		if (CachedHeroInfo != null)
		{
			HeroBonusData data2 = GameRoot.Instance.HeroBonusDataContainer.GetData(CachedHeroInfo.HeroIndex, CachedHeroInfo.Star, CachedHeroInfo.Transcended);
			if (data2 != null)
			{
				LabelIconFieldComponent labelIconFieldComponent2 = UIHelper.AddGridItem(HeroAttributeGrid, LabelIconFieldComponent.PrefabAuto);
				labelIconFieldComponent2.OpenCustom(LocalizationManager.GetString(HeroAttribute.HERO_BONUS), LocalizationManager.GetStringDirect(data2.BonusNameKey), data2.Icon);
				_attributeList.Add(labelIconFieldComponent2);
			}
		}
		UIHelper.RepositionGrid(HeroAttributeGrid);
		if ((bool)HeroAttributeResizeGrid)
		{
			HeroAttributeResizeGrid.Refresh();
		}
	}

	public void SetupStat()
	{
		ClearStat();
		SetupMainStat(CachedHeroInfo, StatGrid, _baseStatList);
		if ((bool)StatResizeGrid)
		{
			StatResizeGrid.Refresh();
		}
	}

	public static void SetupMainStat(CachedHeroInfo cachedHeroInfo, IGrid grid, List<LabelFieldComponent> statList)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(cachedHeroInfo.HeroIndex);
		StatType[] heroMainStats = StatData.GetHeroMainStats((data != null && data.AttrType != null) ? data.AttrType[0] : SkillAttrType.None);
		StatData heroStat = SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(cachedHeroInfo);
		StatData cachedHeroStat = SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(cachedHeroInfo);
		StatType[] array = heroMainStats;
		foreach (StatType statType in array)
		{
			long stat = cachedHeroStat.GetStat(statType);
			long stat2 = heroStat.GetStat(statType);
			long addValue = stat - stat2;
			LabelFieldComponent labelFieldComponent = UIHelper.AddGridItem(grid, LabelFieldComponent.PrefabAuto);
			labelFieldComponent.Open(statType, stat, addValue);
			statList.Add(labelFieldComponent);
		}
		UIHelper.RepositionGrid(grid);
	}

	public void SetupOptions()
	{
		ClearOption();
		SetupOptionStat(CachedHeroInfo, ExtraOptionGrid, _optionStatList);
		UIHelper.EnableGameObject(ExtraOption, _optionStatList.Count > 0);
		if ((bool)ExtraOptionResizeGrid)
		{
			ExtraOptionResizeGrid.Refresh();
		}
	}

	public static void SetupOptionStat(CachedHeroInfo cachedHeroInfo, IGrid grid, List<LabelFieldComponent> statList)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(cachedHeroInfo.HeroIndex);
		SkillAttrType skillAttrType = ((data != null && data.AttrType != null) ? data.AttrType[0] : SkillAttrType.None);
		StatType[] heroMainStats = StatData.GetHeroMainStats(skillAttrType);
		StatData heroStat = SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(cachedHeroInfo);
		StatDataCache.CachedStatData cachedHeroStatDetail = SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStatDetail(cachedHeroInfo);
		foreach (ClientStatDisplayScoreData allDatum in GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetAllData())
		{
			StatType statType = allDatum.StatType;
			if (heroMainStats.Contains(statType) || allDatum.VisibleCreatureTag == null || !allDatum.VisibleCreatureTag.Contains(data.TagType) || (statType == StatType.PhysicalAttack && skillAttrType == SkillAttrType.Magical) || (statType == StatType.MagicalAttack && skillAttrType == SkillAttrType.Physical) || StatData.GetMultedStatTypeOfFactorStat(statType) != StatType.None)
			{
				continue;
			}
			if (allDatum.ShowMultedStatOnly)
			{
				int value = 0;
				Dictionary<int, int> multedStatDictionary = cachedHeroStatDetail.MultedStatDictionary;
				if (multedStatDictionary != null && multedStatDictionary.TryGetValue((int)statType, out value) && value != 0)
				{
					LabelFieldComponent labelFieldComponent = UIHelper.AddGridItem(grid, LabelFieldComponent.PrefabAuto);
					labelFieldComponent.OpenMultedStat(statType, value, value);
					statList.Add(labelFieldComponent);
				}
				continue;
			}
			long stat = heroStat.GetStat(statType);
			long stat2 = cachedHeroStatDetail.StatData.GetStat(statType);
			if (stat != 0L || stat2 != 0L)
			{
				long addValue = stat2 - stat;
				LabelFieldComponent labelFieldComponent2 = UIHelper.AddGridItem(grid, LabelFieldComponent.PrefabAuto);
				labelFieldComponent2.Open(statType, stat2, addValue);
				statList.Add(labelFieldComponent2);
			}
		}
		UIHelper.RepositionGrid(grid);
	}

	private bool HasEnoughItem(int ReqItemCount, CreatureAwakeChallengeItemData ChallengeItemData, bool isTranscend = false)
	{
		for (int i = 1; i <= 3; i++)
		{
			string code = (isTranscend ? ChallengeItemData.GetTranscendItemCode(i) : ChallengeItemData.GetItemCode(i));
			int itemCount = Helper.GetItemCount(GameRoot.Instance.ItemDataContainer.GetDataByCode(code).Index);
			if (ReqItemCount > itemCount)
			{
				return false;
			}
		}
		return true;
	}

	private bool CanStartAwake(AwakeStateType awakeState)
	{
		if (awakeState != AwakeStateType.EnoughMaterial)
		{
			return awakeState == AwakeStateType.NotEnoughMaterial;
		}
		return true;
	}

	protected virtual void SetupAwakeMaterial()
	{
		ClearAwakeMaterial();
		UIHelper.EnableGameObject(WidgetCannotTranscendAnyMore, enable: false);
		UIHelper.EnableGameObject(WidgetAwakeTranscendInfo, enable: false);
		UIHelper.EnableGameObject(WidgetLimitBreakInfo, enable: false);
		UIHelper.EnableGameObject(AwakenTypeRoot, enable: false);
		UIHelper.EnableGameObject(TranscendModeRoot, enable: false);
		UIHelper.EnableGameObject(LimitBreakLevelModeRoot, enable: false);
		UIHelper.EnableGameObject(LimitBreakExpModeRoot, enable: false);
		UIHelper.EnableGameObject(AwakeStatusDesc, enable: true);
		UIHelper.EnableGameObject(AwakeStatusShort, enable: true);
		UIHelper.SetLabelText(AwakeStatusDesc, "");
		UIHelper.SetLabelText(AwakeStatusShort, "");
		if (CachedHeroInfo == null || !_isMine)
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(CachedHeroInfo.HeroIndex);
		if (data == null)
		{
			return;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
		bool num = CachedHeroInfo.IsMaxStar();
		bool flag = CachedHeroInfo.Transcended == valueInt;
		bool flag2 = CachedHeroInfo.CanIncLimitBreakLevel();
		bool flag3 = CachedHeroInfo.CanIncLimitBreakExp();
		bool flag4 = CachedHeroInfo.IsMaxLimitBreakLevel() && !flag2 && !flag3;
		if (!num)
		{
			awakeType = HeroAwakeType.Awaken;
		}
		else if (!flag)
		{
			awakeType = HeroAwakeType.Transcend;
		}
		else
		{
			if (flag4)
			{
				HeroAttributeResizeGrid.ExtraSize.y = 80f;
				HeroAttributeResizeGrid.Refresh();
				UIHelper.RepositionGrid(HeroAttributeGrid);
				Vector3 localPosition = HeroAttributeGrid.transform.localPosition;
				localPosition.y = -88f;
				HeroAttributeGrid.transform.localPosition = localPosition;
				UIHelper.EnableGameObject(Label_BaseInfo, enable: true);
				ReserveRepositionHeroStatTable();
				return;
			}
			awakeType = HeroAwakeType.LimitBreak;
		}
		UIHelper.EnableGameObject(Label_BaseInfo, enable: false);
		HeroAttributeResizeGrid.ExtraSize.y = HeroAttributeExtraSize_Y;
		HeroAttributeResizeGrid.Refresh();
		Vector3 localPosition2 = HeroAttributeGrid.transform.localPosition;
		localPosition2.y = HeroAttributeGrid_Y;
		HeroAttributeGrid.transform.localPosition = localPosition2;
		CreatureAwakeChallengeItemData data2 = GameRoot.Instance.CreatureAwakeChallengeItemDataContainer.GetData(data.TagType);
		if (data2 == null)
		{
			return;
		}
		Action<ItemIconComponent> fallback = null;
		if (!Vespa_Common_Class_Extension_Method.IsUnityNull(_listener))
		{
			fallback = _listener.OnClickCraftItem;
		}
		awakeState = AwakeStateType.None;
		if (awakeType == HeroAwakeType.Awaken)
		{
			UIHelper.EnableGameObject(WidgetAwakeTranscendInfo, enable: true);
			UIHelper.EnableGameObject(AwakenTypeRoot, enable: true);
			int awakeHeroItemCount = HeroTrialDungeonManagement.GetAwakeHeroItemCount(CachedHeroInfo.Star, CachedHeroInfo.Transcended);
			if (CachedHeroInfo.EnterDungeon == 1)
			{
				awakeState = AwakeStateType.InDungeon;
				UIHelper.SetLabelText(AwakeStatusDesc, "[17a0a0]" + LocalizationManager.GetString(HeroStat.IN_BATTLE) + "[-]");
				UIHelper.SetLabelText(AwakeStatusShort, "[17a0a0]" + LocalizationManager.GetString(HeroStat.STATE_AWAKENING) + "[-]");
			}
			else if (NGame2.NAccount.UserHeroManager.CanPurify(CachedHeroInfo))
			{
				awakeState = AwakeStateType.Purifying;
				UIHelper.SetLabelText(AwakeStatusDesc, "[17a0a0]" + LocalizationManager.GetString(HeroStat.IN_PURIFYING) + "[-]");
				UIHelper.SetLabelText(AwakeStatusShort, "[17a0a0]" + LocalizationManager.GetString(HeroStat.STATE_AWAKENING) + "[-]");
			}
			else if (HasEnoughItem(awakeHeroItemCount, data2))
			{
				awakeState = AwakeStateType.EnoughMaterial;
				UIHelper.SetLabelText(AwakeStatusDesc, "[17a0a0]" + LocalizationManager.GetString(HeroStat.ENOUGH_AWAKE_MATERIAL) + "[-]");
				UIHelper.SetLabelText(AwakeStatusShort, "");
			}
			else
			{
				awakeState = AwakeStateType.NotEnoughMaterial;
				UIHelper.SetLabelText(AwakeStatusDesc, "[96866f]" + LocalizationManager.GetString(HeroStat.NOT_ENOUGH_AWAKE_MATERIAL) + "[-]");
				UIHelper.SetLabelText(AwakeStatusShort, "");
			}
			string key = ((awakeState == AwakeStateType.Purifying) ? "HERO_INFO_PURIFY_CONFIRM" : "HERO_INFO_AWAKEN_CONFIRM");
			UIHelper.SetLabelText(Label_AwakeButton, LocalizationManager.GetStringDirect(key));
			UIHelper.EnableButton(ShortCutButton, awakeState == AwakeStateType.EnoughMaterial || awakeState == AwakeStateType.InDungeon || awakeState == AwakeStateType.Purifying);
			UIHelper.EnableButton(TranscendButton, awakeState == AwakeStateType.EnoughMaterial || awakeState == AwakeStateType.InDungeon || awakeState == AwakeStateType.Purifying);
			for (int i = 1; i <= 3; i++)
			{
				string itemCode = data2.GetItemCode(i);
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
				int itemCount = Helper.GetItemCount(dataByCode.Index);
				HeroAwakeItemComponent heroAwakeItemComponent = UIHelper.AddGridItem(AwakeGrid, HeroAwakeItemComponent.PrefabAuto);
				SprkHeroMaterialTickets.Open(heroAwakeItemComponent, dataByCode, awakeHeroItemCount, itemCount, CanStartAwake(awakeState), awakeType, fallback);
				_awakeMaterialList.Add(heroAwakeItemComponent);
			}
			UIHelper.RepositionGrid(AwakeGrid);
		}
		else if (awakeType == HeroAwakeType.Transcend)
		{
			UIHelper.EnableGameObject(WidgetAwakeTranscendInfo, enable: true);
			UIHelper.EnableGameObject(TranscendModeRoot, enable: true);
			int awakeHeroItemCount2 = HeroTrialDungeonManagement.GetAwakeHeroItemCount(CachedHeroInfo.Star, CachedHeroInfo.Transcended);
			CreatureStarData nextData = GameRoot.Instance.CreatureStarDataContainer.GetNextData(CachedHeroInfo.Star, CachedHeroInfo.Transcended);
			int num2 = 0;
			ItemData itemData = ((nextData != null) ? GameRoot.Instance.ItemDataContainer.GetData(nextData.HeroTranscendItemIndex) : null);
			if (itemData != null)
			{
				num2 = Helper.GetItemCount(itemData.Index);
			}
			if (nextData == null)
			{
				awakeState = AwakeStateType.FullyAwakened;
				UIHelper.SetLabelText(AwakeStatusDesc, "[dc097b]" + LocalizationManager.GetString(HeroStat.FULLY_AWAKEN) + "[-]");
				UIHelper.SetLabelText(AwakeStatusShort, "[dc097b]" + LocalizationManager.GetString(HeroStat.STATE_FULLY_TRANSCENDED) + "[-]");
			}
			else if (CachedHeroInfo.EnterDungeon == 1)
			{
				awakeState = AwakeStateType.InDungeon;
				UIHelper.SetLabelText(AwakeStatusDesc, "[17a0a0]" + LocalizationManager.GetString(HeroStat.IN_BATTLE) + "[-]");
				UIHelper.SetLabelText(AwakeStatusShort, "[17a0a0]" + LocalizationManager.GetString(HeroStat.STATE_TRANSCENDING) + "[-]");
			}
			else if (NGame2.NAccount.UserHeroManager.CanTranscendedPurify(CachedHeroInfo))
			{
				awakeState = AwakeStateType.Purifying;
				UIHelper.SetLabelText(AwakeStatusDesc, "[17a0a0]" + LocalizationManager.GetString(HeroStat.IN_TRANSCEND_PURIFYING) + "[-]");
				UIHelper.SetLabelText(AwakeStatusShort, "[17a0a0]" + LocalizationManager.GetString(HeroStat.STATE_TRANSCENDING) + "[-]");
			}
			else if (num2 >= nextData.HeroTranscendItemCount && HasEnoughItem(awakeHeroItemCount2, data2, isTranscend: true))
			{
				awakeState = AwakeStateType.EnoughMaterial;
				UIHelper.SetLabelText(AwakeStatusDesc, "[17a0a0]" + LocalizationManager.GetString(HeroStat.ENOUGH_TRANSCEND_MATERIAL) + "[-]");
				UIHelper.SetLabelText(AwakeStatusShort, "");
			}
			else
			{
				awakeState = AwakeStateType.NotEnoughMaterial;
				UIHelper.SetLabelText(AwakeStatusDesc, "[96866f]" + LocalizationManager.GetString(HeroStat.NOT_ENOUGH_TRANSCEND_MATERIAL) + "[-]");
				UIHelper.SetLabelText(AwakeStatusShort, "");
			}
			string key2 = ((awakeState == AwakeStateType.Purifying) ? "HERO_INFO_PURIFY_CONFIRM" : "HERO_TRANCEND");
			UIHelper.SetLabelText(Label_EnhanceButton, LocalizationManager.GetStringDirect(key2));
			UIHelper.EnableButton(ShortCutButton, awakeState == AwakeStateType.EnoughMaterial || awakeState == AwakeStateType.InDungeon || awakeState == AwakeStateType.Purifying);
			UIHelper.EnableButton(TranscendButton, awakeState == AwakeStateType.EnoughMaterial || awakeState == AwakeStateType.InDungeon || awakeState == AwakeStateType.Purifying);
			if (awakeType == HeroAwakeType.Transcend && itemData != null)
			{
				_transcendMaterial = HeroAwakeItemComponent.PrefabAuto.Factory.Create(TranscendModeRoot);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_transcendMaterial.transform);
				SprkHeroMaterialTickets.Open(_transcendMaterial, itemData, nextData.HeroTranscendItemCount, num2, CanStartAwake(awakeState), awakeType, fallback);
			}
			for (int j = 1; j <= 3; j++)
			{
				string transcendItemCode = data2.GetTranscendItemCode(j);
				ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(transcendItemCode);
				int itemCount2 = Helper.GetItemCount(dataByCode2.Index);
				HeroAwakeItemComponent heroAwakeItemComponent2 = UIHelper.AddGridItem(AwakeGrid, HeroAwakeItemComponent.PrefabAuto);
				SprkHeroMaterialTickets.Open(heroAwakeItemComponent2, dataByCode2, awakeHeroItemCount2, itemCount2, CanStartAwake(awakeState), awakeType, fallback);
				_awakeMaterialList.Add(heroAwakeItemComponent2);
			}
			UIHelper.RepositionGrid(AwakeGrid);
		}
		else if (awakeType == HeroAwakeType.LimitBreak)
		{
			int maxLevel = GameRoot.Instance.CreatureLevelDataContainer.MaxLevel;
			bool flag5 = CachedHeroInfo.Level >= maxLevel;
			if (flag2 || !flag5)
			{
				HeroLimitBreakData data3 = GameRoot.Instance.HeroLimitBreakDataContainer.GetData(new HeroLimitBreakDataKey(CachedHeroInfo.HeroIndex, CachedHeroInfo.LimitBreakLevel + 1));
				if (data3 == null)
				{
					VespaDebug.LogError("LimitBreak error. nextLimitBreakData is null. HeroIndex=" + CachedHeroInfo.HeroIndex + " LimitBreakLevel=" + CachedHeroInfo.LimitBreakLevel);
					return;
				}
				UIHelper.EnableGameObject(WidgetLimitBreakInfo, enable: true);
				UIHelper.EnableGameObject(LimitBreakLevelModeRoot, enable: true);
				bool flag6 = true;
				List<Tuple<string, int>> materialItems = data3.GetMaterialItems();
				for (int k = 0; k < __Extension_Method_ICollection__.COUNT(materialItems); k++)
				{
					string item = materialItems[k].Item1;
					int item2 = materialItems[k].Item2;
					int itemCount3 = Helper.GetItemCount(GameRoot.Instance.ItemDataContainer.GetDataByCode(item).Index);
					flag6 = flag6 && itemCount3 >= item2;
				}
				for (int l = 0; l < __Extension_Method_ICollection__.COUNT(materialItems); l++)
				{
					string item3 = materialItems[l].Item1;
					int item4 = materialItems[l].Item2;
					ItemData dataByCode3 = GameRoot.Instance.ItemDataContainer.GetDataByCode(item3);
					int itemCount4 = Helper.GetItemCount(dataByCode3.Index);
					HeroAwakeItemComponent heroAwakeItemComponent3 = UIHelper.AddGridItem(LimitBreakMaterialGrid, HeroAwakeItemComponent.PrefabAuto);
					SprkHeroMaterialTickets.Open(heroAwakeItemComponent3, dataByCode3, item4, itemCount4, canStart: false, awakeType, null);
					heroAwakeItemComponent3.ShowCountLabel(show: true);
					_awakeMaterialList.Add(heroAwakeItemComponent3);
				}
				UIHelper.RepositionGrid(LimitBreakMaterialGrid);
				if (!flag5)
				{
					awakeState = AwakeStateType.NotEnoughLevel;
					UIHelper.SetLabelText(AwakeStatusDesc, "[96866f]" + LocalizationManager.GetString(HeroStat.NOT_ENOUGH_LEVEL) + "[-]");
					UIHelper.EnableButton(LimitBreakLevelUpButton, enable: false);
				}
				else if (flag6)
				{
					awakeState = AwakeStateType.EnoughMaterial;
					UIHelper.SetLabelText(AwakeStatusDesc, "[17a0a0]" + LocalizationManager.GetString(HeroStat.ENOUGH_LIMIT_BREAK_MATERIAL) + "[-]");
					UIHelper.EnableButton(LimitBreakLevelUpButton, enable: true);
				}
				else
				{
					awakeState = AwakeStateType.NotEnoughMaterial;
					UIHelper.SetLabelText(AwakeStatusDesc, "[96866f]" + LocalizationManager.GetString(HeroStat.NOT_ENOUGH_LIMIT_BREAK_MATERIAL) + "[-]");
					UIHelper.EnableButton(LimitBreakLevelUpButton, enable: false);
				}
			}
			else if (flag3)
			{
				HeroLimitBreakData data4 = GameRoot.Instance.HeroLimitBreakDataContainer.GetData(new HeroLimitBreakDataKey(CachedHeroInfo.HeroIndex, CachedHeroInfo.LimitBreakLevel));
				if (data4 == null)
				{
					VespaDebug.LogError("LimitBreak error. currentLimitBreakData is null. HeroIndex=" + CachedHeroInfo.HeroIndex + " LimitBreakLevel=" + CachedHeroInfo.LimitBreakLevel);
					return;
				}
				UIHelper.EnableGameObject(WidgetLimitBreakInfo, enable: true);
				UIHelper.EnableGameObject(LimitBreakExpModeRoot, enable: true);
				int reqLocalExp = data4.ReqLocalExp;
				long limitBreakExp = CachedHeroInfo.LimitBreakExp;
				float num3 = ((reqLocalExp > 0) ? Mathf.Min(limitBreakExp * 100 / reqLocalExp, 100f) : 0f);
				UIHelper.SetLabelText(Label_LimitBreakExpGauge, num3 + "%");
				UIHelper.SetSpriteFillAmount(Sprite_LimitBreakExpGauge, num3 / 100f);
				bool flag7 = false;
				IList<HeroLimitBreakExpItemData> allData = GameRoot.Instance.HeroLimitBreakExpItemDataContainer.GetAllData();
				if (allData != null)
				{
					foreach (HeroLimitBreakExpItemData item5 in allData)
					{
						if (Helper.GetAccountItemCount(item5.ItemCode) > 0)
						{
							flag7 = true;
							break;
						}
					}
				}
				UIHelper.EnableButton(LimitBreakExpUpButton, flag7);
				if (flag7)
				{
					UIHelper.SetLabelText(AwakeStatusDesc, "[17a0a0]" + LocalizationManager.GetString(HeroStat.ENOUGH_LIMIT_BREAK_EXP_ITEM) + "[-]");
				}
				else
				{
					UIHelper.SetLabelText(AwakeStatusDesc, "[96866f]" + LocalizationManager.GetString(HeroStat.NOT_ENOUGH_LIMIT_BREAK_EXP_ITEM) + "[-]");
				}
			}
		}
		EnterHeroStatAwakenPage enterHeroStatAwakenPage = new EnterHeroStatAwakenPage();
		enterHeroStatAwakenPage.AwakeType = awakeType;
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(enterHeroStatAwakenPage);
	}

	public void OnClickClose()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public void OnClickShortCut()
	{
		if (_listener != null)
		{
			_listener.OnClickShortCut(CachedHeroInfo, awakeState);
		}
	}

	public void OnClickTranscend()
	{
		if (_listener != null)
		{
			_listener.OnClickTranscend(CachedHeroInfo, awakeState);
		}
	}

	public void OnClickLimitBreakLevelUp()
	{
		if (_listener != null)
		{
			_listener.OnClickLimitBreakLevelUp(CachedHeroInfo);
		}
	}

	public void OnClickLimitBreakExpUp()
	{
		if (_listener != null)
		{
			_listener.OnClickLimitBreakExpUp(CachedHeroInfo);
		}
	}

	public AwakeStateType GetAwakeState()
	{
		return awakeState;
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserSoulWeaponManager.SoulWeaponChangedInfo>.OnChanged(NGame2.NAccount.UserSoulWeaponManager.SoulWeaponChangedInfo changedInfo)
	{
		NGame2.NAccount.UserSoulWeaponManager.ESoulWeaponChangedType changedType = changedInfo.ChangedType;
		if (((uint)(changedType - 1) <= 2u || changedType == NGame2.NAccount.UserSoulWeaponManager.ESoulWeaponChangedType.ChangeRatio) && CachedHeroInfo != null)
		{
			SetupStat();
		}
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (CachedHeroInfo == null || changedInfo.HeroIndex == CachedHeroInfo.HeroIndex)
		{
			if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Transcended)
			{
				SetupAwakeMaterial();
				SetupStat();
			}
			else if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLevelup)
			{
				SetupAwakeMaterial();
				SetupStat();
			}
			else if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.EnterTrialDungeon)
			{
				SetupAwakeMaterial();
				SetupStat();
			}
			else if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLimitBreakLevel || changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLimitBreakExp)
			{
				SetupAwakeMaterial();
			}
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		SetupAwakeMaterial();
	}
}
