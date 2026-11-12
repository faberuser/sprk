using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.IAP;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NAccount.NExtension;
using NGame2.NBattleContext;
using NGame2.NContentsDefine;
using NGame2.NDropBonusEvent;
using NGame2.NGuild;
using NGame2.NNodeTree;
using NGame2.NScene;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPortrait;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NEvent;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NUIEffect;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NEntryMenu.NCondition;
using NGame2.NUI.NWindow.NEventdungeon.NCondition;
using NGame2.NUtil;
using NGameAsset;
using NGameAsset.NNguiAtlas;
using NShared;
using NShared.BuyStamina;
using NShared.NLocalization;
using NShared.RechargeStamina;
using NShared.ResetConquestDungeon;
using NShared.ResetHideoutDungeon;
using NShared.UsePotionItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI;

public static class UIHelper
{
	public enum EAssetTextureAssign
	{
		None,
		MakePixelPerfect,
		ClipBottom,
		ClipCenter,
		__Ignore__
	}

	public const string PARTY_SEARCH_CANCEL_WARNING = "PARTY_SEARCH_CANCEL_WARNING";

	public static bool IsEnableGachaTicket(int itemIndex)
	{
		GachaSelectItemData data = GameRoot.Instance.GachaSelectItemDataContainer.GetData(itemIndex);
		if (data == null)
		{
			return false;
		}
		if (data.GachaIndices == null || data.GachaIndices.Length == 0)
		{
			return false;
		}
		bool result = false;
		int[] gachaIndices = data.GachaIndices;
		foreach (int key in gachaIndices)
		{
			NewEquipGachaData data2 = GameRoot.Instance.NewEquipGachaDataContainer.GetData(key);
			EquipGachaInfo equipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Get(key);
			if (equipGachaInfo != null && (equipGachaInfo.OnSale || data2.AllowExpiredTicket))
			{
				if (data2.IsStarterPickupGacha())
				{
					result = true;
				}
				else if (equipGachaInfo.Durational == 0)
				{
					result = true;
				}
				else if (IsEnableDate(equipGachaInfo.BeginTime, equipGachaInfo.EndTime, isUTC: true) || data2.AllowExpiredTicket)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public static List<MonsterHeroInfo> GetMonsterInfoFromDungeon(EnterDungeonData enterDungeon)
	{
		return GetMonsterInfoFromDungeon(enterDungeon.Chapter, enterDungeon.LinkedDungeon, enterDungeon.Difficulty, enterDungeon.IsScenario, enterDungeon.ShowDuplicated);
	}

	public static List<MonsterHeroInfo> GetMonsterInfoFromDungeon(int chapterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty, bool isScenario, bool showDuplicated)
	{
		List<MonsterHeroInfo> list = new List<MonsterHeroInfo>();
		for (int i = 1; i < 100; i++)
		{
			CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, difficulty, isScenario, i);
			if (data == null)
			{
				break;
			}
			foreach (CampaignWaveData.CreatureDetailInfo creature in data.GetCreatureList())
			{
				if (creature.HidePortrait || (!showDuplicated && list.Find(delegate(MonsterHeroInfo e)
				{
					MonsterGradeType grade = e.Grade;
					MonsterGradeType grade2 = creature.Grade;
					return e.HeroInfo.HeroIndex == creature.Index && e.HeroInfo.Star == creature.Star && e.HeroInfo.Level == creature.Level && grade == grade2;
				}) != null))
				{
					continue;
				}
				MonsterHeroInfo monsterHeroInfo = new MonsterHeroInfo();
				monsterHeroInfo.HeroInfo = new HeroInfo();
				monsterHeroInfo.HeroInfo.HeroIndex = creature.Index;
				monsterHeroInfo.HeroInfo.Star = (sbyte)creature.Star;
				monsterHeroInfo.HeroInfo.Level = (short)creature.Level;
				monsterHeroInfo.Tier = creature.Tier;
				monsterHeroInfo.Grade = creature.Grade;
				monsterHeroInfo.IsBoss = creature.IsBoss;
				monsterHeroInfo.VisualInfo = creature.VisualInfo;
				monsterHeroInfo.WaveSlotIndex = creature.WaveSlotIndex;
				if (creature.IsBoss)
				{
					int num = list.FindIndex((MonsterHeroInfo monster) => !monster.IsBoss);
					if (num == -1)
					{
						list.Add(monsterHeroInfo);
					}
					else
					{
						list.Insert(num, monsterHeroInfo);
					}
				}
				else
				{
					list.Add(monsterHeroInfo);
				}
			}
		}
		list.Sort(delegate(MonsterHeroInfo l, MonsterHeroInfo r)
		{
			if (l.IsBoss && !r.IsBoss)
			{
				return -1;
			}
			if (!l.IsBoss && r.IsBoss)
			{
				return 1;
			}
			MonsterGradeType grade = l.Grade;
			MonsterGradeType grade2 = r.Grade;
			int num2 = -grade.CompareTo(grade2);
			return (num2 != 0) ? num2 : l.HeroInfo.HeroIndex.CompareTo(r.HeroInfo.HeroIndex);
		});
		return list;
	}

	public static List<Component> CreateSmallMonsterPortraitList(UIScrollGrid grid, EnterDungeonData enterDungeon)
	{
		List<Component> list = new List<Component>();
		List<MonsterHeroInfo> monsterInfoFromDungeon = GetMonsterInfoFromDungeon(enterDungeon);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(monsterInfoFromDungeon); i++)
		{
			MonsterHeroInfo monsterHeroInfo = monsterInfoFromDungeon[i];
			HeroPortraitComponent heroPortraitComponent = AddGridItem(grid, HeroPortraitComponent.SmallPrefabAuto);
			if (!(heroPortraitComponent == null))
			{
				CachedHeroInfo heroInfo = new CachedHeroInfo(monsterHeroInfo);
				if (heroPortraitComponent.Open(heroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon))
				{
					list.Add(heroPortraitComponent);
				}
			}
		}
		return list;
	}

	public static List<Component> CreateMonsterPortraitList(UIScrollGrid grid, EnterDungeonData enterDungeon, int questMonsterIndex, bool isShowHP = false)
	{
		List<Component> list = new List<Component>();
		List<MonsterHeroInfo> monsterInfoFromDungeon = GetMonsterInfoFromDungeon(enterDungeon);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(monsterInfoFromDungeon); i++)
		{
			MonsterHeroInfo monsterHeroInfo = monsterInfoFromDungeon[i];
			if (monsterHeroInfo.Grade == MonsterGradeType.Hero)
			{
				HeroPortraitComponent heroPortraitComponent = AddGridItem(grid, HeroPortraitComponent.ShortPrefabAuto);
				if (!(heroPortraitComponent == null))
				{
					CachedHeroInfo heroInfo = new CachedHeroInfo(monsterHeroInfo);
					if (heroPortraitComponent.Open(heroInfo, null))
					{
						list.Add(heroPortraitComponent);
					}
				}
				continue;
			}
			MonsterComponent monsterComponent = AddGridItem(grid, MonsterComponent.PrefabAuto);
			if (!(monsterComponent == null))
			{
				MonsterComponent.StockData stock = new MonsterComponent.StockData
				{
					ShowQuestTag = (questMonsterIndex != 0 && questMonsterIndex == monsterHeroInfo.HeroInfo.HeroIndex),
					IsHidden = false
				};
				monsterComponent.Open(monsterHeroInfo, stock, null, bDisableClick: false, isShowHP);
				list.Add(monsterComponent);
			}
		}
		RepositionGrid(grid);
		return list;
	}

	public static ItemIconComponent CreateRewardMainItemIconComponent(int rewardIndex, Transform iconRoot, UILabel nameLabel = null)
	{
		return CreateRewardMainItemIconComponent(rewardIndex, iconRoot, null, nameLabel);
	}

	public static ItemIconComponent CreateRewardMainItemIconComponent(int rewardIndex, Transform iconRoot, ItemIconComponent.Listener listener, UILabel nameLabel = null)
	{
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null)
		{
			return null;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		if (__Extension_Method_ICollection__.COUNT(rewardItemList) == 0)
		{
			VespaDebug.LogError("npcFriendly reward nothing");
			return null;
		}
		RewardElementData rewardElementData = rewardItemList[0];
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
		if (dataByCode == null)
		{
			VespaDebug.LogError("npcFriendly reward Item nothing");
			return null;
		}
		ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(iconRoot);
		if (dataByCode.Type == ItemType.Equip)
		{
			int itemStarMin = rewardElementData.ItemStarMin;
			EquipItemInfo equipItemInfo = new EquipItemInfo
			{
				ItemIndex = dataByCode.Index,
				Star = itemStarMin
			};
			itemIconComponent.Open(dataByCode, equipItemInfo, listener);
		}
		else
		{
			int itemMin = rewardElementData.ItemMin;
			long count = itemMin;
			itemIconComponent.Open(dataByCode, listener);
			itemIconComponent.SetItemCount(count);
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
		SetLabelText(nameLabel, dataByCode.Name);
		return itemIconComponent;
	}

	public static List<Component> CreateRewardItemIconComponentList(int[] rewardInices, UIBaseGrid grid, bool includeCurrency = false)
	{
		List<Component> list = new List<Component>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardInices); i++)
		{
			int key = rewardInices[i];
			RewardData data = GameRoot.Instance.RewardDataContainer.GetData(key);
			list.AddRange(CreateRewardItemIconComponentList(data, grid, includeCurrency));
		}
		return list;
	}

	public static List<Component> CreateRewardItemIconComponentList(int rewardIndex, UIBaseGrid grid, bool includeCurrency = false)
	{
		return CreateRewardItemIconComponentList(GameRoot.Instance.RewardDataContainer.GetData(rewardIndex), grid, includeCurrency);
	}

	public static List<Component> CreateRewardItemIconComponentList(RewardData rewardData, UIBaseGrid grid, bool includeCurrency = false)
	{
		List<Component> list = new List<Component>();
		if (rewardData == null)
		{
			return list;
		}
		List<RewardElementData> rewardItemList = rewardData.GetRewardItemList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemList); i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			if (includeCurrency)
			{
				CurrencyType currencyType = rewardElementData.GetCurrencyType();
				if (currencyType != CurrencyType.None)
				{
					CurrencyIconComponent currencyIconComponent = AddGridItem(grid, CurrencyIconComponent.PrefabAuto);
					int itemMax = rewardElementData.ItemMax;
					CurrencyIconComponent.Listener listener = null;
					int num = itemMax;
					currencyIconComponent.Open(currencyType, num, listener);
					list.Add(currencyIconComponent);
					continue;
				}
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode == null || dataByCode.Type == ItemType.Hero)
			{
				continue;
			}
			ItemIconComponent itemIconComponent = grid.AddGridItem(ItemIconComponent.PrefabAuto);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.Open(dataByCode);
				if (dataByCode.Type != ItemType.Equip)
				{
					int itemMin = rewardElementData.ItemMin;
					int itemMax2 = rewardElementData.ItemMax;
					Color? color = null;
					int num2 = itemMax2;
					int num3 = itemMin;
					itemIconComponent.SetItemMinMax(num3, num2, color);
				}
				list.Add(itemIconComponent);
			}
		}
		return list;
	}

	public static List<Component> CreateInfinityRewardItemIconComponentList(int rewardGroupIndex, int level, UIBaseGrid grid)
	{
		InfinityRewardGroupData data = GameRoot.Instance.InfinityRewardGroupDataContainer.GetData(rewardGroupIndex);
		if (data == null)
		{
			return null;
		}
		int[] rewardIndices = data.RewardIndices;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(rewardIndices))
		{
			return null;
		}
		List<Component> list = new List<Component>();
		for (int i = 0; i < rewardIndices.Length; i++)
		{
			Component item = CreateInfinityRewardItemIconComponent(rewardIndices[i], level, grid);
			list.Add(item);
		}
		return list;
	}

	public static Component CreateInfinityRewardItemIconComponent(int infinityRewardIndex, int level, UIBaseGrid grid)
	{
		InfinityRewardData data = GameRoot.Instance.InfinityRewardDataContainer.GetData(infinityRewardIndex);
		if (data == null || data.RewardType == RewardType.None || data.RewardType == RewardType.Handler)
		{
			return null;
		}
		RewardType rewardType = data.RewardType;
		if (rewardType == RewardType.Item)
		{
			int itemIndex = data.ItemIndex;
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data2 == null)
			{
				return null;
			}
			int infinityIndex = data.InfinityIndex;
			int sumValue = GameRoot.Instance.InfinityDataContainer.GetSumValue(infinityIndex, level);
			long num = sumValue;
			if (num == 0L)
			{
				return null;
			}
			ItemIconComponent itemIconComponent = AddGridItem(grid, ItemIconComponent.PrefabAuto);
			if (itemIconComponent == null)
			{
				return null;
			}
			itemIconComponent.Open(data2);
			if (data.IsRatio)
			{
				itemIconComponent.SetItemRatio(num);
			}
			else
			{
				itemIconComponent.SetItemCount(num);
			}
			itemIconComponent.ResetPosition();
			return itemIconComponent;
		}
		CurrencyType currencyType = GetCurrencyType(rewardType);
		if (currencyType == CurrencyType.None)
		{
			return null;
		}
		int infinityIndex2 = data.InfinityIndex;
		int sumValue2 = GameRoot.Instance.InfinityDataContainer.GetSumValue(infinityIndex2, level);
		long num2 = sumValue2;
		if (num2 == 0L)
		{
			return null;
		}
		CurrencyIconComponent currencyIconComponent = AddGridItem(grid, CurrencyIconComponent.PrefabAuto);
		currencyIconComponent.Open(currencyType, num2, null);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(currencyIconComponent.transform);
		return currencyIconComponent;
	}

	public static void SetWidgetColor(UIWidget uiWidget, Color color)
	{
		if (!(uiWidget == null) && uiWidget.color != color)
		{
			uiWidget.color = color;
		}
	}

	public static void SetWidgetAlpha(UIWidget uiWidget, float alpha)
	{
		if (!(uiWidget == null))
		{
			uiWidget.alpha = alpha;
		}
	}

	public static void EnableGameObject(GameObject gameObject, bool enable, bool isForce = false)
	{
		if (!(gameObject == null))
		{
			if (isForce)
			{
				gameObject.SetActive(!enable);
			}
			gameObject.SetActive(enable);
		}
	}

	public static void EnableGameObject(IList<GameObject> gameObjects, bool enable, bool isForce = false)
	{
		if (gameObjects == null)
		{
			return;
		}
		foreach (GameObject gameObject in gameObjects)
		{
			EnableGameObject(gameObject, enable, isForce);
		}
	}

	public static void EnableGameObject(GameObject gameObject, Vector3 localPosition, bool enable, bool isForce = false)
	{
		if (!(gameObject == null))
		{
			gameObject.transform.localPosition = localPosition;
			EnableGameObject(gameObject, enable, isForce);
		}
	}

	public static void EnableGameObject(Component component, bool enable, bool isForce = false)
	{
		if (!(component == null))
		{
			EnableGameObject(component.gameObject, enable, isForce);
		}
	}

	public static void ToggleGameObject(GameObject gameObject)
	{
		if (!(gameObject == null))
		{
			EnableGameObject(gameObject, !gameObject.activeInHierarchy);
		}
	}

	public static bool IsEnabledGameObjet(GameObject gameObject)
	{
		if ((bool)gameObject)
		{
			return gameObject.activeInHierarchy;
		}
		return false;
	}

	public static bool IsEnabledGameObjet(Component component)
	{
		if (component == null)
		{
			return false;
		}
		return component.gameObject.activeInHierarchy;
	}

	public static bool IsEnabledSelfGameObjet(Component component)
	{
		if (component == null)
		{
			return false;
		}
		return IsEnabledSelfGameObjet(component.gameObject);
	}

	public static bool IsEnabledSelfGameObjet(GameObject gameObject)
	{
		if ((bool)gameObject)
		{
			return gameObject.activeSelf;
		}
		return false;
	}

	public static void SetLabelText(UILabel label, object text, bool unwrapNewline = false)
	{
		if (!(label == null))
		{
			if (text != null && SingletonAuto<LocalizationManager>.isValidInstance && SingletonAuto<LocalizationManager>.instance.Language == SystemLanguage.Thai)
			{
				text = ThaiFontAdjuster.Adjust(text.ToString());
			}
			if (text != null && unwrapNewline)
			{
				text = text.ToString().Replace("\\n", "\n");
			}
			label.text = ((text != null) ? text.ToString() : string.Empty);
		}
	}

	public static void SetLabelText(UILabel label, Enum enumValue)
	{
		string text = LocalizationManager.GetString(enumValue);
		if (string.IsNullOrEmpty(text))
		{
			text = enumValue.ToString();
		}
		SetLabelText(label, text);
	}

	public static void SetLabelTextLocalizeDirect(UILabel label, string localeKey)
	{
		string stringDirect = LocalizationManager.GetStringDirect(localeKey);
		SetLabelText(label, stringDirect);
	}

	public static void SetLabelText(UILabelAnimator label, int from, int to)
	{
		if (label != null)
		{
			label.Animate(from, to);
		}
	}

	public static void SetLabelEncoding(UILabel label, bool supportEncoding)
	{
		if (!(label == null))
		{
			label.supportEncoding = supportEncoding;
		}
	}

	public static void SetLabelColor(UILabel label, Color color)
	{
		if (!(label == null))
		{
			label.color = color;
		}
	}

	public static void SetLabelEffectColor(UILabel label, Color color)
	{
		if (!(label == null))
		{
			label.effectColor = color;
		}
	}

	public static void SetWidgetGrayScale(UIButton button, bool grayScale, bool setColliders = false)
	{
		if (button == null)
		{
			return;
		}
		UIWidget component = button.GetComponent<UIWidget>();
		if (!(component == null))
		{
			bool flag = component.isGrayscale != grayScale;
			component.isGrayscale = grayScale;
			if (setColliders)
			{
				EnableCollidersInChildren(component, !grayScale);
			}
			if (flag && component.gameObject.activeSelf)
			{
				EnableGameObject(component, enable: false);
				EnableGameObject(component, enable: true);
			}
		}
	}

	public static void SetWidgetGrayScale(UIWidget widget, bool grayScale, bool setColliders = false)
	{
		if (!(widget == null))
		{
			bool num = widget.isGrayscale != grayScale;
			widget.isGrayscale = grayScale;
			if (setColliders)
			{
				EnableCollidersInChildren(widget, !grayScale);
			}
			if (num && widget.gameObject.activeSelf)
			{
				EnableGameObject(widget, enable: false);
				EnableGameObject(widget, enable: true);
			}
		}
	}

	public static void SetLabelForNoBillingInfo(UILabel labelPrice, UILabel labelCurrency)
	{
		SetLabelText(labelPrice, "------");
		SetLabelText(labelCurrency, string.Empty);
	}

	public static void SetLabelForBillingInfo(UILabel labelPrice, UILabel labelCurrency, SkuInfo skuInfo)
	{
		if (skuInfo == null)
		{
			SetLabelForNoBillingInfo(labelPrice, labelCurrency);
		}
		else if (!skuInfo.Purchased)
		{
			SetLabelText(labelPrice, skuInfo.price);
			SetLabelText(labelCurrency, skuInfo.priceCurrencyCode);
		}
		else
		{
			string text = LocalizationManager.GetString(Billing.PURCHASE_ERROR);
			SetLabelText(labelPrice, text);
			SetLabelText(labelCurrency, string.Empty);
		}
	}

	public static string GetRankLabelText(VisualTierRank visualTierRank)
	{
		string text = "";
		if (visualTierRank.Value == 1)
		{
			return visualTierRank.Value + LocalizationManager.GetString(Match.RANK_POSTFIX_1);
		}
		if (visualTierRank.Value == 2)
		{
			return visualTierRank.Value + LocalizationManager.GetString(Match.RANK_POSTFIX_2);
		}
		if (visualTierRank.Value == 3)
		{
			return visualTierRank.Value + LocalizationManager.GetString(Match.RANK_POSTFIX_3);
		}
		return visualTierRank.Value + LocalizationManager.GetString(Match.RANK_POSTFIX);
	}

	public static void SetRankLabelText(UILabel label, VisualTierRank visualTierRank)
	{
		SetLabelText(label, GetRankLabelText(visualTierRank));
	}

	public static string GetElapseTimeLabelText(long milliSeconds)
	{
		TimeSpan timeSpan = new TimeSpan(0, 0, 0, (int)(milliSeconds / 1000), (int)(milliSeconds % 1000));
		int num = timeSpan.Milliseconds % 1000;
		int num2 = num % 100;
		num /= 100;
		num2 /= 10;
		return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{num}{num2}";
	}

	public static string GetRemainTimeLabelText(long remainTime)
	{
		return GetRemainTimeLabelText(new TimeSpan(0, 0, checked((int)remainTime)));
	}

	public static string GetRemainTimeLabelText(TimeSpan ts)
	{
		string empty = string.Empty;
		if (ts.Days > 0)
		{
			return LocalizationManager.GetStringDirect("CONTENTS_REMAIN_TIME_DAY_HOUR", ts.Days, ts.Hours);
		}
		if (ts.Hours > 0)
		{
			return LocalizationManager.GetStringDirect("CONTENTS_REMAIN_TIME_HOUR_MINUTE", ts.Hours, ts.Minutes);
		}
		return LocalizationManager.GetStringDirect("CONTENTS_REMAIN_TIME_MINUTE_SECOND", ts.Minutes, ts.Seconds);
	}

	public static string GetRemainTimeText(int remainTime)
	{
		string empty = string.Empty;
		TimeSpan timeSpan = new TimeSpan(0, 0, remainTime);
		if (timeSpan.Days > 0)
		{
			return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_DAY_HOUR, timeSpan.Days, timeSpan.Hours);
		}
		if (timeSpan.Hours > 0)
		{
			return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_HOUR_MINUTE, timeSpan.Hours, timeSpan.Minutes);
		}
		if (timeSpan.Minutes > 0)
		{
			return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_MIN_SEC, timeSpan.Minutes, timeSpan.Seconds);
		}
		return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_SEC, timeSpan.Seconds);
	}

	public static string GetRemainTimeTextAllDelZero(int remainTime)
	{
		StringBuilder stringBuilder = new StringBuilder();
		TimeSpan timeSpan = new TimeSpan(0, 0, remainTime);
		if (timeSpan.TotalDays >= 1.0)
		{
			stringBuilder.Append(LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_DAY, timeSpan.Days));
		}
		if (timeSpan.TotalHours >= 1.0 && (int)(timeSpan.TotalHours % 24.0) != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_HOUR, timeSpan.Hours));
		}
		if (timeSpan.TotalMinutes >= 1.0 && (int)(timeSpan.TotalMinutes % 60.0) != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_MIN, timeSpan.Minutes));
		}
		if (timeSpan.TotalSeconds >= 1.0 && (int)(timeSpan.TotalSeconds % 60.0) != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_SEC, timeSpan.Seconds));
		}
		return stringBuilder.ToString();
	}

	public static string GetRemainTimeTextAll(long remainTime, bool showDay = true, bool showHour = true, bool showMin = true, bool showSec = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		TimeSpan timeSpan = new TimeSpan(0, 0, checked((int)remainTime));
		if (showDay && timeSpan.TotalDays >= 1.0)
		{
			stringBuilder.Append(LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_DAY, timeSpan.Days));
		}
		if (showHour && timeSpan.TotalHours >= 1.0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_HOUR, timeSpan.Hours));
		}
		if (showMin && timeSpan.TotalMinutes >= 1.0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_MIN, timeSpan.Minutes));
		}
		if (showSec || timeSpan.TotalMinutes < 1.0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_SEC, timeSpan.Seconds));
		}
		return stringBuilder.ToString();
	}

	public static string GetRemainTimeTextSingle(int remainTime)
	{
		string empty = string.Empty;
		TimeSpan timeSpan = new TimeSpan(0, 0, remainTime);
		if (timeSpan.Days > 0)
		{
			return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_DAY, timeSpan.Days);
		}
		if (timeSpan.Hours > 0)
		{
			return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_HOUR, timeSpan.Hours);
		}
		if (timeSpan.Minutes > 0)
		{
			return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_MIN, timeSpan.Minutes);
		}
		return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_SEC, timeSpan.Seconds);
	}

	public static string GetRemainHourTextFromSecond(int remainSecond)
	{
		string empty = string.Empty;
		int hours = remainSecond / 3600;
		TimeSpan timeSpan = new TimeSpan(hours, 0, 0);
		if (timeSpan.Days > 0)
		{
			return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_DAY_HOUR, timeSpan.Days, timeSpan.Hours);
		}
		return LocalizationManager.GetString(NShared.NLocalization.System.LABEL_REMAIN_HOUR, timeSpan.Hours);
	}

	public static void SetRemainTimeLabelText(UILabel label, int remainTime)
	{
		SetLabelText(label, GetRemainTimeLabelText(remainTime));
	}

	public static void ReleaseTextureInfo(UITexture uiTex)
	{
		if (!(uiTex == null))
		{
			uiTex.mainTexture = null;
			uiTex.material = null;
		}
	}

	public static void SetTextureInfo(UITexture uiTexture, AssetTexture.TextureInfo texInfo, EAssetTextureAssign assetTexAssign = EAssetTextureAssign.None)
	{
		if (uiTexture == null)
		{
			return;
		}
		if (texInfo == null)
		{
			uiTexture.mainTexture = null;
			uiTexture.material = null;
			uiTexture.uvRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);
			return;
		}
		uiTexture.mainTexture = texInfo.Tex;
		uiTexture.material = texInfo.Mtrl;
		uiTexture.uvRect = texInfo.UvRect;
		switch (assetTexAssign)
		{
		case EAssetTextureAssign.MakePixelPerfect:
			uiTexture.width = texInfo.Width;
			uiTexture.height = texInfo.Height;
			break;
		case EAssetTextureAssign.ClipBottom:
		{
			int num4 = uiTexture.height * texInfo.Width / uiTexture.width;
			if (num4 < texInfo.Height && texInfo.Height > 0)
			{
				float t3 = (float)num4 / (float)texInfo.Height;
				float ymin2 = Mathf.Lerp(texInfo.UvRect.yMax, texInfo.UvRect.yMin, t3);
				uiTexture.uvRect = Rect.MinMaxRect(texInfo.UvRect.xMin, ymin2, texInfo.UvRect.xMax, texInfo.UvRect.yMax);
			}
			break;
		}
		case EAssetTextureAssign.ClipCenter:
		{
			int num = uiTexture.height * texInfo.Width / uiTexture.width;
			if (num < texInfo.Height && texInfo.Height > 0)
			{
				float num2 = (float)(texInfo.Height + num) / 2f;
				float num3 = (float)(texInfo.Height - num) / 2f;
				float t = num2 / (float)texInfo.Height;
				float t2 = num3 / (float)texInfo.Height;
				float ymin = Mathf.Lerp(texInfo.UvRect.yMax, texInfo.UvRect.yMin, t);
				float ymax = Mathf.Lerp(texInfo.UvRect.yMax, texInfo.UvRect.yMin, t2);
				uiTexture.uvRect = Rect.MinMaxRect(texInfo.UvRect.xMin, ymin, texInfo.UvRect.xMax, ymax);
			}
			break;
		}
		}
	}

	public static void SetTexture(UITexture uiTexture, Texture texture)
	{
		if (!(uiTexture == null))
		{
			if (texture == null)
			{
				SetTextureMaterial(uiTexture, null);
				return;
			}
			uiTexture.mainTexture = texture;
			uiTexture.material = null;
			uiTexture.uvRect = new Rect(0f, 0f, 1f, 1f);
		}
	}

	public static void SetTextureMaterial(UITexture uiTexture, Material mtrl)
	{
		if (!(uiTexture == null))
		{
			uiTexture.material = mtrl;
			if (mtrl != null)
			{
				uiTexture.mainTexture = mtrl.mainTexture;
			}
			else
			{
				uiTexture.mainTexture = null;
			}
		}
	}

	public static void SetTextureFillAmount(UITexture tex, float fillAmount)
	{
		if (!(tex == null))
		{
			tex.fillAmount = fillAmount;
		}
	}

	public static void SetSpriteFillAmount(UISprite sprite, float fillAmount)
	{
		if (!(sprite == null))
		{
			sprite.fillAmount = fillAmount;
		}
	}

	public static void SetSpriteFillAmount(UIClipSprite sprite, float fillAmount)
	{
		if (!(sprite == null))
		{
			sprite.ClipFillAmount = fillAmount;
		}
	}

	public static void SetGrayscaleSprite(UISprite sprite, bool isGray)
	{
		sprite.isGrayscale = isGray;
		EnableGameObject(sprite, enable: false);
		EnableGameObject(sprite, enable: true);
	}

	public static void SetSpriteName(UISprite sprite, string spriteName)
	{
		if (!(sprite == null))
		{
			sprite.spriteName = spriteName;
		}
	}

	public static void EnableButton(UIButton button, bool enable, bool grayscaleIfDisable = false, bool forceGrayScale = false)
	{
		if (button == null)
		{
			return;
		}
		button.isEnabled = enable;
		if (!(grayscaleIfDisable || forceGrayScale))
		{
			return;
		}
		UIWidget component = button.GetComponent<UIWidget>();
		if ((bool)component)
		{
			bool flag = !enable || forceGrayScale;
			bool num = component.isGrayscale != flag;
			component.isGrayscale = flag;
			if (num && component.gameObject.activeSelf)
			{
				EnableGameObject(component, enable: false);
				EnableGameObject(component, enable: true);
			}
		}
	}

	public static void ResetGridPosition(UIGrid grid)
	{
		if (!(grid == null))
		{
			grid.Reposition();
			UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(grid.gameObject);
			if ((bool)uIScrollView)
			{
				uIScrollView.ResetPosition();
			}
		}
	}

	public static void MoveGridPositionToSelectedItem(UIGrid grid, Vector3 targetChild, int step = 0)
	{
		if (grid == null)
		{
			return;
		}
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(grid.gameObject);
		if (!uIScrollView)
		{
			return;
		}
		uIScrollView.ResetPosition();
		if (step == 0)
		{
			uIScrollView.MoveRelative(-targetChild);
		}
		else if (step < 0)
		{
			if (grid.arrangement == UIGrid.Arrangement.Vertical)
			{
				float cellHeight = grid.cellHeight;
				float num = uIScrollView.panel.height - cellHeight;
				float num2 = cellHeight * (float)(grid.GetGridChildCount() - 1);
				if (System.Math.Abs(targetChild.y) >= num2)
				{
					Vector3 vector = targetChild + new Vector3(0f, num, 0f);
					uIScrollView.MoveRelative(-vector);
				}
				else
				{
					Vector3 vector2 = targetChild + new Vector3(0f, num + cellHeight * (float)step, 0f);
					uIScrollView.MoveRelative(-vector2);
				}
			}
			else if (grid.arrangement == UIGrid.Arrangement.Horizontal)
			{
				float cellWidth = grid.cellWidth;
				float num3 = uIScrollView.panel.width - cellWidth;
				float num4 = cellWidth * (float)(grid.GetGridChildCount() - 1);
				if (System.Math.Abs(targetChild.x) > num4)
				{
					Vector3 vector3 = targetChild + new Vector3(0f, num3, 0f);
					uIScrollView.MoveRelative(-vector3);
				}
				else
				{
					Vector3 vector4 = targetChild + new Vector3(num3 + cellWidth * (float)step, 0f, 0f);
					uIScrollView.MoveRelative(-vector4);
				}
			}
		}
		else if (grid.arrangement == UIGrid.Arrangement.Vertical)
		{
			float cellHeight2 = grid.cellHeight;
			if (targetChild.y == 0f)
			{
				uIScrollView.MoveRelative(-targetChild);
				return;
			}
			Vector3 vector5 = targetChild + new Vector3(0f, cellHeight2 * (float)step, 0f);
			uIScrollView.MoveRelative(-vector5);
		}
		else if (grid.arrangement == UIGrid.Arrangement.Horizontal)
		{
			float cellWidth2 = grid.cellWidth;
			if (targetChild.x == 0f)
			{
				uIScrollView.MoveRelative(-targetChild);
				return;
			}
			Vector3 vector6 = targetChild + new Vector3(cellWidth2 * (float)step, 0f, 0f);
			uIScrollView.MoveRelative(-vector6);
		}
	}

	public static void GridOriginPositioning(UIGrid grid)
	{
		if (grid == null)
		{
			return;
		}
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(grid.gameObject);
		if (!(uIScrollView == null))
		{
			if (grid.arrangement == UIGrid.Arrangement.Vertical)
			{
				float cellHeight = grid.cellHeight;
				float height = uIScrollView.panel.height;
				grid.transform.localPosition = new Vector3(0f, (height - cellHeight) / 2f, 0f);
			}
			else if (grid.arrangement == UIGrid.Arrangement.Horizontal)
			{
				float cellWidth = grid.cellWidth;
				float width = uIScrollView.panel.width;
				grid.transform.localPosition = new Vector3((0f - (width - cellWidth)) / 2f, 0f, 0f);
			}
			uIScrollView.ResetPosition();
		}
	}

	public static void CheckOutOfGridSightItem(UIGrid grid, Vector3 targetChild, Action outOfNearSight, Action outOfFarSight)
	{
		if (grid == null)
		{
			return;
		}
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(grid.gameObject);
		if (uIScrollView == null)
		{
			return;
		}
		Vector2 clipOffset = uIScrollView.panel.clipOffset;
		if (grid.arrangement == UIGrid.Arrangement.Vertical)
		{
			_ = grid.cellHeight;
			float height = uIScrollView.panel.height;
			if (System.Math.Abs(clipOffset.y) > System.Math.Abs(targetChild.y))
			{
				Vespa_System_Action_Extension_Method.fire(outOfNearSight);
			}
			else if (System.Math.Abs(clipOffset.y) + height <= System.Math.Abs(targetChild.y))
			{
				Vespa_System_Action_Extension_Method.fire(outOfFarSight);
			}
		}
		else if (grid.arrangement == UIGrid.Arrangement.Horizontal)
		{
			_ = grid.cellWidth;
			float width = uIScrollView.panel.width;
			if (System.Math.Abs(clipOffset.x) > System.Math.Abs(targetChild.x))
			{
				Vespa_System_Action_Extension_Method.fire(outOfNearSight);
			}
			else if (System.Math.Abs(clipOffset.x) + width <= System.Math.Abs(targetChild.x))
			{
				Vespa_System_Action_Extension_Method.fire(outOfFarSight);
			}
		}
	}

	public static void ResetGridPosition(UIScrollGrid scrollGrid)
	{
		if (!(scrollGrid == null))
		{
			RepositionGrid(scrollGrid);
			if (scrollGrid.ScrollView != null)
			{
				scrollGrid.ScrollView.ResetPosition();
			}
		}
	}

	public static void RepositionGrid(IGrid Grid)
	{
		Grid?.RepositionGrid();
	}

	public static T AddGridItem<T>(IGrid grid, PrefabAuto<T> prefabAuto) where T : Component
	{
		if (grid == null)
		{
			return null;
		}
		return grid.AddGridItem(prefabAuto);
	}

	public static string GetHeroPortraitPath(string heroCode, int Star = 0, int costumeIndex = 0)
	{
		string text = heroCode.Replace('.', '/');
		if (costumeIndex > 0)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
			if (data != null)
			{
				return data.PortraitPath;
			}
		}
		if (Star > 0)
		{
			text = text + "_" + Star;
		}
		return "Portrait/" + text;
	}

	public static string GetHeroIconPath(HeroInfo heroInfo, string postfix = "")
	{
		return GetHeroIconPath(heroInfo.HeroIndex, heroInfo.Star, postfix, heroInfo.CostumeIndex);
	}

	public static string GetHeroIconPath(string heroCode, int star, string postfix = "", int costumeIndex = 0)
	{
		string text = heroCode.Replace('.', '/');
		if (costumeIndex > 0)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
			if (data != null)
			{
				return data.IconPortraitPath;
			}
		}
		if (star == 0)
		{
			return "Icon/Creature" + postfix + "/" + text;
		}
		return $"Icon/Creature{postfix}/{text}_{star}";
	}

	public static string GetHeroIconPath(int heroIndex, int star, string postfix = "", int costumeIndex = 0)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return string.Empty;
		}
		if (data.StartHeroStar == 0)
		{
			return GetHeroIconPath(data.CodeName, 0, postfix, costumeIndex);
		}
		return GetHeroIconPath(data.CodeName, Mathf.Max(data.StartHeroStar, star), postfix, costumeIndex);
	}

	public static string GetHeroIconPath(int heroIndex, bool ignoreAccount = false)
	{
		int heroStar = Helper.GetHeroStar(heroIndex, ignoreAccount);
		return GetHeroIconPath(heroIndex, heroStar);
	}

	public static string GetHeroLargeIconPath(string heroCode, int star)
	{
		return GetHeroIconPath(heroCode, star, "Large");
	}

	public static string GetHeroFullPortraitPath(string heroCode)
	{
		string text = heroCode.Replace("Hero.", "HeroFull/");
		return "Portrait/" + text;
	}

	public static string GetHeroBackgroundPath(string heroCode)
	{
		string text = heroCode.Replace("Hero.", "HeroFull_BG/");
		return "Portrait/" + text;
	}

	public static string GetBattleHeroPortraitPath(HeroInfo heroInfo)
	{
		CreatureData creatureData = heroInfo.CreatureData;
		if (creatureData == null)
		{
			return string.Empty;
		}
		return GetBattleHeroPortraitPath(creatureData.CodeName, heroInfo.Star, heroInfo.CostumeIndex);
	}

	public static string GetBattleHeroPortraitPath(string heroCode, int star, int costumeIndex)
	{
		string text = SprkImageRecovery.BattlePortraitFallback(heroCode, star, costumeIndex);
		if (text != null)
		{
			return text;
		}
		string text2 = heroCode.Replace('.', '/');
		if (costumeIndex > 0)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
			if (data != null)
			{
				return data.BattlePortraitPath;
			}
		}
		if (star == 0)
		{
			return String_Extension_Method.CombinePath("Battle/Portrait", text2);
		}
		return $"Battle/Portrait/{text2}_{star}";
	}

	public static string GetBattleHeroPortraitEffectPath(string heroCode, int star, int costumeIndex)
	{
		heroCode.Replace('.', '/');
		if (costumeIndex > 0)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
			if (data != null)
			{
				return data.BattlePortraitEffectPath;
			}
		}
		return string.Empty;
	}

	public static string GetChapterPortraitPath(int chapterIndex)
	{
		return $"Portrait/Dictionary/Field/Chapter{chapterIndex}";
	}

	public static string GetPetPortraitPath(int petIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(petIndex);
		if (data == null)
		{
			return string.Empty;
		}
		string arg = data.CodeName.Replace('.', '_').ToUpper();
		return $"Portrait/Pet/{arg}";
	}

	public static string GetPetIconPath(int petIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(petIndex);
		if (data == null)
		{
			return string.Empty;
		}
		string arg = data.CodeName.Replace('.', '_').ToUpper();
		return $"Icon/Creature/Pet/{arg}_MINI";
	}

	public static string GetPetIncubatorIconPath(int incubatorIndex)
	{
		PetIncubatorData data = GameRoot.Instance.PetIncubatorDataContainer.GetData(incubatorIndex);
		if (data == null)
		{
			return string.Empty;
		}
		string arg = data.IconPath.Replace('.', '_').ToUpper();
		return $"Icon/Item/{arg}";
	}

	public static string GetCreatureIconPath(string creatureCode)
	{
		string arg = creatureCode.Replace('.', '/');
		return $"Icon/Creature/{arg}";
	}

	public static string GetBossBigPortraitPath(string heroCode)
	{
		string arg = heroCode.Replace('.', '/');
		return $"Portrait/{arg}";
	}

	public static string GetMonsterCirclePortraitPath(string heroCode, int Star = 0)
	{
		string text = heroCode.Replace('.', '/');
		if (Star > 0)
		{
			text = text + "_" + Star;
		}
		return "Portrait/Circle/" + text;
	}

	public static AssetTexture GetMonsterCirclePortraitAsset(string heroCode, int Star = 0)
	{
		string monsterCirclePortraitPath = GetMonsterCirclePortraitPath(heroCode, Star);
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(monsterCirclePortraitPath);
		return assetTexture;
	}

	public static string GetGamePlayTipImagePath(string tipName)
	{
		return "LoadingTip/GamePlayTip/" + tipName;
	}

	public static string GetItemIconPath(int itemIndex)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data != null)
		{
			return GetItemIconPath(data.Code);
		}
		return string.Empty;
	}

	public static string GetItemIconPath(string itemCode)
	{
		if (itemCode == "SPRK_CLASS_UNIQUE_WEAPON_SELECTOR")
		{
			return "Icon/Item/WEAPON_UNIQUE_SELECT_ITEM";
		}
		if (itemCode == "SPRK_COSTUME_SELECT_LIGHT_102")
		{
			return "Icon/Item/COSTUME_SELECT_ITEM_97_LIGHT_BALANCE_210427";
		}
		if (itemCode == null)
		{
			return string.Empty;
		}
		string text = itemCode.Replace('.', '/');
		return "Icon/Item/" + text;
	}

	public static string GetSkillIconPath(int skillIndex, HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return GetSkillIconPathOrigin(skillIndex);
		}
		int costumeIndex = heroInfo.CostumeIndex;
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		if (data == null)
		{
			return GetSkillIconPathOrigin(skillIndex);
		}
		if (costumeIndex != 0 && data.CostumeType == EHeroCostumeType.Legend)
		{
			return $"Icon/Skill/{skillIndex}_{costumeIndex}";
		}
		return GetSkillIconPathOrigin(skillIndex);
	}

	public static string GetSkillIconPathOrigin(int skillIndex)
	{
		return $"Icon/Skill/{skillIndex}";
	}

	public static string GetBuffIconPath(string iconPath, HeroInfo heroInfo)
	{
		int legendCostumeIndex = GetLegendCostumeIndex(heroInfo);
		if (legendCostumeIndex == 0)
		{
			return iconPath;
		}
		if (SingletonAuto<AtlasDataContainer>.instance == null)
		{
			return iconPath;
		}
		string text = $"{iconPath}_{legendCostumeIndex}";
		if (SingletonAuto<AtlasDataContainer>.instance.GetAtlasLink(text) == null)
		{
			return iconPath;
		}
		return text;
	}

	private static int GetLegendCostumeIndex(HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return 0;
		}
		int costumeIndex = heroInfo.CostumeIndex;
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		if (data == null)
		{
			return 0;
		}
		if (costumeIndex <= 0 || data.CostumeType != EHeroCostumeType.Legend)
		{
			return 0;
		}
		return costumeIndex;
	}

	public static string GetGuildLogoIconPath(int logoIndex)
	{
		return "Icon/GuildLogo/Logo/" + logoIndex;
	}

	public static string GetGuildBackGround2DPath(int bgIndex)
	{
		return "Icon/GuildFlag/" + bgIndex;
	}

	public static string GetGuildBackGround2DLongPath(int bgIndex)
	{
		return "Icon/GuildFlagLong/" + bgIndex;
	}

	public static string GetGuildLogo3dBgIconPath(int logoBgIndex)
	{
		return "Icon/GuildFlagBg3D/" + logoBgIndex;
	}

	public static string GetAchievementTypeIconPath(string achievementType)
	{
		return "Icon/Achievement/" + achievementType;
	}

	public static string GetPortalBackgroundPath(string code)
	{
		if (String_Extension_Method.IsNullOrEmpty(code))
		{
			return string.Empty;
		}
		string text = code.Replace('.', '/');
		return "Portal/" + text;
	}

	public static string GetBoosterIconPath(string itemCode)
	{
		return "Booster/" + itemCode;
	}

	public static string GetCountryFlagIconPath(string countryCode)
	{
		return "Country/" + countryCode;
	}

	public static string GetAttrTypeIconPath(string skillType)
	{
		return "Icon/AttrType/" + skillType;
	}

	public static void SetBundleTexture(UIBundleLoader bundleTex, string assetPath, EAssetTextureAssign assetTexAssign = EAssetTextureAssign.__Ignore__)
	{
		if (bundleTex != null)
		{
			bundleTex.SetAssetPath(assetPath, assetTexAssign);
		}
	}

	public static void SetBundleTextureGrayscale(UIBundleLoader bundleTex, bool isGrayscale)
	{
		if (bundleTex != null)
		{
			SetWidgetGrayScale(bundleTex.UiTex, isGrayscale);
		}
	}

	public static void ClearPlaneAssetTexture(UITexture uiTex = null)
	{
		if (!(uiTex == null))
		{
			PlainAssetHolder component = uiTex.GetComponent<PlainAssetHolder>();
			if (component != null)
			{
				component.Close();
			}
		}
	}

	public static PlainAssetHolder SetPlaneAssetTextureAsync(Component componentRoot, UITexture uiTex, AssetTexture assetTex, EAssetTextureAssign assetTexAssign = EAssetTextureAssign.None, Action<AssetTexture.TextureInfo> onAssign = null)
	{
		if (assetTex == null || uiTex == null)
		{
			assetTex?.Close();
			Vespa_System_Action_Extension_Method.fire(onAssign, null);
			return null;
		}
		uiTex.material = null;
		uiTex.mainTexture = null;
		PlainAssetHolder componentAuto = Vespa_Unity_Component_Extension_Method.GetComponentAuto<PlainAssetHolder>(uiTex);
		componentAuto.Open(componentRoot, uiTex, assetTex, assetTexAssign, onAssign);
		return componentAuto;
	}

	public static PlainAssetHolder ResetPlaneAssetTextureAsync(Component componentRoot, UITexture uiTex, AssetTexture assetTex, EAssetTextureAssign assetTexAssign = EAssetTextureAssign.None, Action<AssetTexture.TextureInfo> onAssign = null)
	{
		ClearPlaneAssetTexture(uiTex);
		return SetPlaneAssetTextureAsync(componentRoot, uiTex, assetTex, assetTexAssign, onAssign);
	}

	public static PlainAssetHolder SetPlaneAssetTextureAsync(Component componentRoot, UITexture uiTex, string assetPath, EAssetTextureAssign assetTexAssign = EAssetTextureAssign.None, Action<AssetTexture.TextureInfo> onAssign = null)
	{
		if (String_Extension_Method.IsNullOrEmpty(assetPath) || uiTex == null)
		{
			Vespa_System_Action_Extension_Method.fire(onAssign, null);
			return null;
		}
		uiTex.material = null;
		uiTex.mainTexture = null;
		PlainAssetHolder componentAuto = Vespa_Unity_Component_Extension_Method.GetComponentAuto<PlainAssetHolder>(uiTex);
		componentAuto.Open(componentRoot, uiTex, assetPath, assetTexAssign, onAssign);
		return componentAuto;
	}

	public static PlainAssetHolder ResetPlaneAssetTextureAsync(Component componentRoot, UITexture uiTex, string assetPath, EAssetTextureAssign assetTexAssign = EAssetTextureAssign.None, Action<AssetTexture.TextureInfo> onAssign = null)
	{
		ClearPlaneAssetTexture(uiTex);
		return SetPlaneAssetTextureAsync(componentRoot, uiTex, assetPath, assetTexAssign, onAssign);
	}

	public static AssetTexture GetPlainTexture(string path)
	{
		if (String_Extension_Method.IsNullOrEmpty(path))
		{
			return null;
		}
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open(path);
		return assetTexture;
	}

	public static AssetTexture GetHeroIcon2(HeroInfo heroInfo)
	{
		return GetPlainTexture(GetHeroIconPath(heroInfo));
	}

	public static AssetTexture GetHeroIcon2(string heroCode, int star)
	{
		return GetPlainTexture(GetHeroIconPath(heroCode, star));
	}

	public static AssetTexture GetHeroIcon2(int heroIndex, int star)
	{
		return GetPlainTexture(GetHeroIconPath(heroIndex, star));
	}

	public static AssetTexture GetHeroIcon2(int heroIndex, bool ignoreAccount = false)
	{
		return GetPlainTexture(GetHeroIconPath(heroIndex, ignoreAccount));
	}

	public static AssetTexture GetHeroLargeIcon2(string heroCode, int star)
	{
		return GetPlainTexture(GetHeroLargeIconPath(heroCode, star));
	}

	public static AssetTexture GetHeroFullPortrait(string heroCode)
	{
		return GetPlainTexture(GetHeroFullPortraitPath(heroCode));
	}

	public static AssetTexture GetHeroBackgroundImage(string heroCode)
	{
		return GetPlainTexture(GetHeroBackgroundPath(heroCode));
	}

	public static AssetTexture GetGamePlayTipImage(string tipName)
	{
		return GetPlainTexture(GetGamePlayTipImagePath(tipName));
	}

	public static AssetTexture GetItemIcon2(int itemIndex)
	{
		return GetPlainTexture(GetItemIconPath(itemIndex));
	}

	public static AssetTexture GetItemIcon2(string itemCode)
	{
		return GetPlainTexture(GetItemIconPath(itemCode));
	}

	public static AssetTexture GetSkillIcon2(int skillIndex, HeroInfo heroInfo)
	{
		string text = GetSkillIconPath(skillIndex, heroInfo);
		if (SingletonAuto<AtlasDataContainer>.instance == null)
		{
			return null;
		}
		if (SingletonAuto<AtlasDataContainer>.instance.GetAtlasLink(text) == null)
		{
			text = GetSkillIconPathOrigin(skillIndex);
			if (SingletonAuto<AtlasDataContainer>.instance.GetAtlasLink(text) == null)
			{
				return null;
			}
		}
		return GetPlainTexture(text);
	}

	public static AssetTexture GetGuildLogoIcon2(int logoIndex)
	{
		return GetPlainTexture(GetGuildLogoIconPath(logoIndex));
	}

	public static AssetTexture GetGuildBackGround2D(int bgIndex)
	{
		return GetPlainTexture(GetGuildBackGround2DPath(bgIndex));
	}

	public static AssetTexture GetGuildBackGround2DLong(int bgIndex)
	{
		return GetPlainTexture(GetGuildBackGround2DLongPath(bgIndex));
	}

	public static AssetTexture GetAchievementTypeIcon(string achievementType)
	{
		return GetPlainTexture(GetAchievementTypeIconPath(achievementType));
	}

	public static AssetTexture GetPortalBackground(string code)
	{
		return GetPlainTexture(GetPortalBackgroundPath(code));
	}

	public static AssetTexture GetBoosterIcon(string itemCode)
	{
		return GetPlainTexture(GetBoosterIconPath(itemCode));
	}

	public static AssetTexture GetCountryFlagIcon(string countryCode)
	{
		return GetPlainTexture(GetCountryFlagIconPath(countryCode));
	}

	public static AssetTexture GetAttrTypeIcon(string skillType)
	{
		return GetPlainTexture(GetAttrTypeIconPath(skillType));
	}

	public static void SetDungeonPortrait(Component owner, UIBundleLoader bundleTexture, int chapterIndex, int dungeonIndex)
	{
		PartyDungeonData data = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			SetDungeonPortrait(bundleTexture, data);
		}
		else
		{
			RaidHelper.InitPortrait(owner, bundleTexture, chapterIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy);
		}
	}

	public static void SetDungeonPortrait(UIBundleLoader bundleTexture, PartyDungeonData partyDungeonData)
	{
		string portraitPath = partyDungeonData.PortraitPath;
		if (String_Extension_Method.IsNullOrEmpty(portraitPath))
		{
			VespaDebug.LogError("portraitPath.IsNullOrEmpty()");
		}
		SetBundleTexture(bundleTexture, portraitPath);
	}

	public static void Play3DWidgetEffect(UIGame3DWidget Game3DWidget, bool CheckPlay = true, Action onComplete = null)
	{
		if (!(Game3DWidget == null))
		{
			if (CheckPlay && Game3DWidget.IsPlaying)
			{
				Game3DWidget.Stop(immediatelyClear: true);
			}
			Game3DWidget.Play(onComplete);
		}
	}

	public static void CloseHero3dPreview(ref HeroPreview preview, UITexture texture)
	{
		if (preview != null)
		{
			HeroPreview.ReleaseInstance(preview);
			preview = null;
			texture.mainTexture = null;
		}
	}

	public static void InitHero3dPreview(ref HeroPreview preview, UITexture texture, int heroIndex, int costumeIndex, RenderFacadeBuilder renderFacadeBuilder, string backgroundName, bool perspective)
	{
		if (texture == null)
		{
			return;
		}
		HeroPreview.InfoData info = default(HeroPreview.InfoData);
		SingletonAuto<ResolutionInfo>.instance.CalcRenderTargetSize(texture.width, texture.height, out info.RenderTargetWidth, out info.RenderTargetHeight);
		info.BackgroundName = backgroundName;
		info.RenderFacadeBuilder = renderFacadeBuilder;
		if (preview != null && preview.Info.IsEquals(info))
		{
			return;
		}
		CloseHero3dPreview(ref preview, texture);
		preview = HeroPreview.CreateInstance(info);
		if (preview != null)
		{
			ClientCreatureData clientCreatureDataByIndex = DataHelper.GetClientCreatureDataByIndex(heroIndex, costumeIndex);
			if (perspective)
			{
				preview.PreviewCamera = preview.DefaultPerspective;
				preview.DefaultPerspective.ZoomOutRate = 0f;
				preview.DefaultPerspective.ZoomIn_Offset = ((clientCreatureDataByIndex != null) ? NVespa.Math.Vec3FromArray(clientCreatureDataByIndex.DetailViewZoomInOffset) : Vector3.zero);
				preview.DefaultPerspective.ZoomIn_Distance = clientCreatureDataByIndex?.DetailViewZoomInDistance ?? 2f;
				preview.DefaultPerspective.ZoomOut_Offset = ((clientCreatureDataByIndex != null) ? NVespa.Math.Vec3FromArray(clientCreatureDataByIndex.DetailViewZoomOutOffset) : Vector3.zero);
				preview.DefaultPerspective.ZoomOut_Distance = clientCreatureDataByIndex?.DetailViewZoomOutDistance ?? 10f;
				preview.DefaultPerspective.LookAngleX = GameSetting.data.Ui.Hero3dView.CameraAngle;
			}
			else
			{
				preview.PreviewCamera = preview.DefaultOrtho;
				preview.DefaultOrtho.ZoomOutRate = 0f;
				preview.DefaultOrtho.ZoomIn_Offset = ((clientCreatureDataByIndex != null) ? NVespa.Math.Vec3FromArray(clientCreatureDataByIndex.HeroViewZoomInOffset) : Vector3.zero);
				preview.DefaultOrtho.ZoomOut_Offset = ((clientCreatureDataByIndex != null) ? NVespa.Math.Vec3FromArray(clientCreatureDataByIndex.HeroViewZoomOutOffset) : Vector3.zero);
				preview.DefaultOrtho.ZoomIn_ViewSize = clientCreatureDataByIndex?.HeroViewZoomInViewSize ?? 0.5f;
				preview.DefaultOrtho.ZoomOut_ViewSize = clientCreatureDataByIndex?.HeroViewZoomOutViewSize ?? 0.5f;
				preview.DefaultOrtho.LookAngleX = GameSetting.data.Ui.Hero3dView.CameraAngle;
			}
			texture.mainTexture = preview.RenderTarget;
		}
	}

	public static void CloseArchive3dPreview(ref HeroPreview preview, UITexture texture)
	{
		if (preview != null)
		{
			HeroPreview.ReleaseInstance(preview);
			preview = null;
			texture.mainTexture = null;
		}
	}

	public static void InitArchive3dPreview(ref HeroPreview preview, UITexture texture, int heroIndex, int costumeIndex, RenderFacadeBuilder renderFacadeBuilder, string backgroundName)
	{
		if (texture == null)
		{
			return;
		}
		HeroPreview.InfoData info = default(HeroPreview.InfoData);
		SingletonAuto<ResolutionInfo>.instance.CalcRenderTargetSize(texture.width, texture.height, out info.RenderTargetWidth, out info.RenderTargetHeight);
		info.BackgroundName = backgroundName;
		info.RenderFacadeBuilder = renderFacadeBuilder;
		if (!(preview != null) || !preview.Info.IsEquals(info))
		{
			CloseArchive3dPreview(ref preview, texture);
			preview = HeroPreview.CreateInstance(info);
			if (preview != null)
			{
				ClientCreatureData clientCreatureDataByIndex = DataHelper.GetClientCreatureDataByIndex(heroIndex, costumeIndex);
				preview.PreviewCamera = preview.DefaultPerspective;
				preview.DefaultPerspective.ZoomOutRate = 0f;
				preview.DefaultPerspective.ZoomIn_Offset = ((clientCreatureDataByIndex != null) ? NVespa.Math.Vec3FromArray(clientCreatureDataByIndex.ArchiveViewZoomInOffset) : Vector3.zero);
				preview.DefaultPerspective.ZoomIn_Distance = clientCreatureDataByIndex?.ArchiveViewZoomInDistance ?? 2f;
				preview.DefaultPerspective.ZoomOut_Offset = ((clientCreatureDataByIndex != null) ? NVespa.Math.Vec3FromArray(clientCreatureDataByIndex.ArchiveViewZoomOutOffset) : Vector3.zero);
				preview.DefaultPerspective.ZoomOut_Distance = clientCreatureDataByIndex?.ArchiveViewZoomOutDistance ?? 10f;
				preview.DefaultPerspective.LookAngleX = GameSetting.data.Ui.Hero3dView.CameraAngle;
				texture.mainTexture = preview.RenderTarget;
			}
		}
	}

	private static void UseStaminaPotion(int itemIndex, int useCount, Action onSuccess)
	{
		SendUsePotion(itemIndex, useCount, 0, onSuccess);
	}

	private static void SendUsePotion(int itemIndex, int useCount, int chapterIndex = 0, Action onSuccess = null)
	{
		NShared.UsePotionItem.Request request = new NShared.UsePotionItem.Request
		{
			ItemIndex = itemIndex,
			ItemCount = useCount,
			ChapterIndex = ((chapterIndex != 0) ? chapterIndex : 0)
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePotionItem.Response response)
		{
			if (response.Result != NShared.UsePotionItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				StaminaType o_value = ((response.StaminaResult != null) ? response.StaminaResult.Type : StaminaType.None);
				long num2;
				if (response.StaminaResult == null)
				{
					int num = 0;
					num2 = num;
				}
				else
				{
					num2 = response.StaminaResult.AddValue;
				}
				long num3 = num2;
				if (num3 == 0L)
				{
					ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(itemIndex);
					int num4 = data?.SubData.ActionValue ?? 0;
					long num5 = num4;
					string str = ((data != null) ? data.SubData.ActionSubValue : string.Empty);
					int num6 = useCount;
					num3 = checked(num5 * num6);
					if (!EnumTools.TryParse<StaminaType>(str, out o_value))
					{
						o_value = StaminaType.Chicken;
					}
				}
				GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.Apply(response.HideoutDungeonInfo);
				GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.Apply(response.ConquestDungeonInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Apply(response.ItemTimeDurationInfos);
				string text = LocalizationManager.GetString(o_value);
				string message = LocalizationManager.GetString(Item.USE_POTION_MSG, text, num3);
				MultiCenterMessages multiCenterMessages = MultiCenterMessages.Open();
				if (o_value == StaminaType.EclipseKey && EclipseKeyGaugeComponent.ShowEclipseKeyGauge())
				{
					multiCenterMessages.DelayTime = 3f;
					multiCenterMessages.ShowEclipseGauge();
				}
				multiCenterMessages.AddIconMessage(message, o_value.ToString());
				if (response.ItemTimeDurationInfos != null)
				{
					for (int i = 0; i < response.ItemTimeDurationInfos.Length; i++)
					{
						string boosterApplyString = GetBoosterApplyString(response.ItemTimeDurationInfos[i]);
						if (!String_Extension_Method.IsNullOrEmpty(boosterApplyString))
						{
							multiCenterMessages.AddMessage(boosterApplyString);
						}
					}
				}
				multiCenterMessages.RepositionMessage();
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	public static string GetBoosterApplyString(PlayerItemTimeDurationInfo itemTimeDurationInfo)
	{
		BoosterItemData boosterDataByGroupIndex = GameRoot.Instance.BoosterItemDataContainer.GetBoosterDataByGroupIndex(itemTimeDurationInfo.ItemIndex);
		if (boosterDataByGroupIndex == null)
		{
			return null;
		}
		EventBonusType type = boosterDataByGroupIndex.Type;
		int value = boosterDataByGroupIndex.Value;
		TimeSpan remainTime = itemTimeDurationInfo.GetRemainTime();
		string boosterBonusTypeString = GetBoosterBonusTypeString(type);
		if (String_Extension_Method.IsNullOrEmpty(boosterBonusTypeString))
		{
			return null;
		}
		return LocalizationManager.GetString(Item.BOOSTER_EFFECT_APPLY_MSG, boosterBonusTypeString, value, (int)remainTime.TotalDays, remainTime.Hours, remainTime.Minutes);
	}

	private static void RechargeStamina(Action onSuccess)
	{
		StaminaType staminaType = StaminaType.Chicken;
		NShared.BuyStamina.Request request = new NShared.BuyStamina.Request
		{
			StaminaType = staminaType
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BuyStamina.Response response)
		{
			if (response.Result != NShared.BuyStamina.ResultType.Success)
			{
				if (response.Result == NShared.BuyStamina.ResultType.NotEnoughGem)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					MessagePopup.Open(title, message, delegate
					{
						if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
						{
							SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
						}
						else if (SceneAutoBehaviour<EndCampaign>.isValidInstance)
						{
							SceneAutoBehaviour<EndCampaign>.instance.ExecReturnToLobbyAndOpenCashShop();
						}
					}, null);
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("BuyStamina");
				string text = LocalizationManager.GetString(staminaType);
				IconCenterMessage.Open(LocalizationManager.GetString(Item.USE_POTION_MSG, text, valueInt), staminaType);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	public static void RechargeKey(StaminaType staminaType, Action onSuccess)
	{
		NShared.RechargeStamina.Request request = new NShared.RechargeStamina.Request
		{
			StaminaType = staminaType
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RechargeStamina.Response response)
		{
			if (response.Result != NShared.RechargeStamina.ResultType.Success)
			{
				if (staminaType == StaminaType.TechnoEnchantKey && response.Result == NShared.RechargeStamina.ResultType.ResetLimitExceeded)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string stringDirect = LocalizationManager.GetStringDirect("GODKING_TRIAL.NOT_ENTER");
					MessagePopup.Open(title, stringDirect);
				}
				else if (response.Result == NShared.RechargeStamina.ResultType.NotEnoughGem)
				{
					string title2 = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					MessagePopup.Open(title2, message, delegate
					{
						SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
					}, null);
				}
				else if (response.Result == NShared.RechargeStamina.ResultType.MaxStamina)
				{
					MessagePopup.OpenError(response.Result);
					long maxStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(staminaType);
					int num = 0;
					if (maxStamina > num)
					{
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetStamina(staminaType, maxStamina);
					}
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	private static void RechargeHideDungeonKey(int chapterIndex, Action onSuccess)
	{
		NShared.ResetHideoutDungeon.Request request = new NShared.ResetHideoutDungeon.Request
		{
			ChapterIndex = chapterIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ResetHideoutDungeon.Response response)
		{
			if (response.Result != NShared.ResetHideoutDungeon.ResultType.Success)
			{
				if (response.Result == NShared.ResetHideoutDungeon.ResultType.NotEnoughGem)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					MessagePopup.Open(title, message, delegate
					{
						SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
					}, null);
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.Apply(response.HideoutDungeonInfo);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	public static void ResoveRecoverGem(IList<int> forcedItemIndices)
	{
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Potion);
		List<Tuple<ItemData, int>> gemPotionList = new List<Tuple<ItemData, int>>();
		foreach (ItemInfo item in itemsByType)
		{
			if (forcedItemIndices == null || forcedItemIndices.Contains(item.ItemIndex))
			{
				ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(item.ItemIndex);
				if (data != null && data.SubData.ActionType == ItemActionType.RecoverGem)
				{
					gemPotionList.Add(new Tuple<ItemData, int>(data, item.Count));
				}
			}
		}
		if (forcedItemIndices != null && __Extension_Method_ICollection__.COUNT(gemPotionList) == 0)
		{
			VespaDebug.LogError("No gem potion from forcedItemIndices");
		}
		else
		{
			if (gemPotionList.Count <= 0)
			{
				return;
			}
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string name = gemPotionList.First().Item1.Name;
			string stringDirect = LocalizationManager.GetStringDirect("INVEN_USE_STAMINA3", name);
			SelectItemMessagePopup.Open(title, stringDirect, gemPotionList, delegate(SelectItemMessagePopup selectItemPopup, int idx, int count)
			{
				Tuple<ItemData, int> tuple = gemPotionList.Find((Tuple<ItemData, int> e) => e.Item1.Index == idx);
				if (tuple == null)
				{
					VespaDebug.LogError("Selected potion not found : idx=" + idx);
				}
				else
				{
					ItemData<PotionItemData> potionItemData = tuple.Item1 as ItemData<PotionItemData>;
					if (potionItemData == null)
					{
						VespaDebug.LogError("Invalid potion item data : idx=" + idx + " name=" + tuple.Item1.Name);
					}
					else
					{
						string name2 = potionItemData.Name;
						int actionValue = potionItemData.SubData.ActionValue;
						int recoverValue = actionValue * count;
						string stringDirect2 = LocalizationManager.GetStringDirect("COST_TYPE_RUBY");
						string message = LocalizationManager.GetString(ItemUse.USE_RUBYFLASK_ITEM_CONFIRM, name2, count, recoverValue, stringDirect2);
						string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
						string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
						IconConfirmPopup popup = IconConfirmPopup.Open(message, confirmLabel, cancelLabel, null);
						popup.SetConfirmCallback(delegate
						{
							selectItemPopup.Close();
							NShared.UsePotionItem.Request request = new NShared.UsePotionItem.Request
							{
								ItemIndex = idx,
								ItemCount = count
							};
							SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePotionItem.Response response)
							{
								if (response.Result != NShared.UsePotionItem.ResultType.Success)
								{
									popup.Close();
									MessagePopup.OpenError(response.Result);
								}
								else
								{
									Action callback = delegate
									{
										popup.Close();
										GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
										GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
										GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
										GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Apply(response.ItemTimeDurationInfos);
										message = LocalizationManager.GetStringDirect("Item.USE_RUBY_FLASK_MSG", recoverValue);
										MultiCenterMessages multiCenterMessages = MultiCenterMessages.Open();
										multiCenterMessages.AddIconMessage(message, "Gem");
										multiCenterMessages.RepositionMessage();
									};
									string uIEffectPrefabName = potionItemData.SubData.UIEffectPrefabName;
									string[] uIEffectValue = potionItemData.SubData.UIEffectValue;
									UIEffectUtility.PlayEffectAndCallback(uIEffectPrefabName, uIEffectValue, GameUIRoot.instance.TopmostPanel.transform, callback);
								}
							});
						});
						popup.AddItem(idx, count);
						popup.SetManualClose(manualClose: true);
					}
				}
			}, null, closeOnYes: false);
		}
	}

	private static void RechargeConquestDungeonKey(int chapterIndex, Action onSuccess)
	{
		NShared.ResetConquestDungeon.Request request = new NShared.ResetConquestDungeon.Request
		{
			ChapterIndex = chapterIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ResetConquestDungeon.Response response)
		{
			if (response.Result != NShared.ResetConquestDungeon.ResultType.Success)
			{
				if (response.Result == NShared.ResetConquestDungeon.ResultType.NotEnoughGem)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					MessagePopup.Open(title, message, delegate
					{
						SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
					}, null);
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.Apply(response.ConquestDungeonInfo);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	private static void GetStaminaLocale(StaminaType staminaType, bool isNotEnough, out string title, out string content, out string staminaName)
	{
		title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		content = string.Empty;
		staminaName = LocalizationManager.GetString(staminaType);
		if (isNotEnough)
		{
			content = LocalizationManager.GetStringDirect("INVEN_USE_STAMINA", staminaName);
		}
		else if (staminaType == StaminaType.Chicken)
		{
			content = LocalizationManager.GetStringDirect("INVEN_USE_STAMINA2", staminaName);
		}
		else
		{
			content = LocalizationManager.GetStringDirect("INVEN_USE_STAMINA3", staminaName);
		}
	}

	public static void ShowSelectItemMessagePopup(bool isNotEnough, StaminaType staminaType, List<Tuple<ItemData, int>> staminaPotionList, int reqChapterIndex, Action onSuccess = null, bool isSkip = true)
	{
		GetStaminaLocale(staminaType, isNotEnough, out var title, out var content, out var staminaName);
		SelectItemMessagePopup.Open(title, content, staminaPotionList, delegate(SelectItemMessagePopup selectItemPopup, int idx, int count)
		{
			if (staminaType == StaminaType.HideoutKey || staminaType == StaminaType.ConquestKey)
			{
				if (isNotEnough)
				{
					ShowItemConfirmPopup(selectItemPopup, staminaPotionList, staminaName, idx, count, reqChapterIndex, onSuccess);
				}
				else
				{
					PortalChildListPopup.OpenDungeonListPopup(staminaType, delegate(int chapterIdx, int dungeonIdx)
					{
						ShowItemConfirmPopup(selectItemPopup, staminaPotionList, staminaName, idx, count, chapterIdx, onSuccess);
					}, delegate
					{
						CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
					});
				}
			}
			else
			{
				ShowItemConfirmPopup(selectItemPopup, staminaPotionList, staminaName, idx, count, 0, onSuccess);
			}
		}, delegate
		{
			if (isNotEnough)
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.IsKeyTypeDungeon(staminaType))
				{
					ResolveNotEnoughKey(staminaType, onSuccess, reqChapterIndex);
				}
				else if (!isSkip)
				{
					ResolveNotEnoughStaminaConfirm(staminaType, onSuccess, reqChapterIndex);
				}
				else
				{
					ResolveNotEnoughStamina(staminaType, onSuccess, reqChapterIndex);
				}
			}
		}, closeOnYes: false);
	}

	public static void ShowItemConfirmPopup(SelectItemMessagePopup selectItemPopup, List<Tuple<ItemData, int>> staminaPotionList, string staminaName, int idx, int count, int chapterIdx = 0, Action onSuccess = null)
	{
		Tuple<ItemData, int> tuple = staminaPotionList.Find((Tuple<ItemData, int> e) => e.Item1.Index == idx);
		if (tuple == null)
		{
			VespaDebug.LogError("Selected potion not found : idx=" + idx);
			return;
		}
		if (!(tuple.Item1 is ItemData<PotionItemData> potionItemData))
		{
			VespaDebug.LogError("Invalid potion item data : idx=" + idx + " name=" + tuple.Item1.Name);
			return;
		}
		string potionEffectContent = GetPotionEffectContent(potionItemData, staminaName, count);
		string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
		IconConfirmPopup.Open(potionEffectContent, confirmLabel, cancelLabel, delegate
		{
			selectItemPopup.Close();
			if (chapterIdx != 0)
			{
				PortalChildListPopup portalChildListPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<PortalChildListPopup>();
				if (portalChildListPopup != null)
				{
					portalChildListPopup.Close();
				}
				SendUsePotion(idx, count, chapterIdx, onSuccess);
			}
			else
			{
				UseStaminaPotion(idx, count, onSuccess);
			}
		}).AddItem(idx, count);
	}

	public static void ResolveRecoverStamina(bool isNotEnough, IList<int> forcedItemIndices, StaminaType staminaType, int reqChapterIndex = 0, Action onSuccess = null, bool isSkip = true)
	{
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Potion);
		List<Tuple<ItemData, int>> list = new List<Tuple<ItemData, int>>();
		foreach (ItemInfo item in itemsByType)
		{
			if (forcedItemIndices != null && !forcedItemIndices.Contains(item.ItemIndex))
			{
				continue;
			}
			ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(item.ItemIndex);
			if (data != null && (data.SubData.ActionType == ItemActionType.RecoverStamina || data.SubData.ActionType == ItemActionType.RecoverStaminaPercent || data.SubData.ActionType == ItemActionType.RecoverHideoutKey || data.SubData.ActionType == ItemActionType.RecoverConquestKey))
			{
				if (!EnumTools.TryParse<StaminaType>(data.SubData.ActionSubValue, out var o_value))
				{
					o_value = StaminaType.Chicken;
				}
				if (o_value == staminaType)
				{
					list.Add(new Tuple<ItemData, int>(data, item.Count));
				}
			}
		}
		if (forcedItemIndices != null && __Extension_Method_ICollection__.COUNT(list) == 0)
		{
			VespaDebug.LogError("No stamina potion from forcedItemIndices");
		}
		else if (list.Count > 0)
		{
			ShowSelectItemMessagePopup(isNotEnough, staminaType, list, reqChapterIndex, onSuccess, isSkip);
		}
		else if (staminaType == StaminaType.Chicken)
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("BuyStaminaGem");
			int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("BuyStamina");
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("STAMINA_RECHARGE_MESSAGE", valueInt, valueInt2);
			string stringDirect2 = LocalizationManager.GetStringDirect("RECHARGE_ARENATICKET");
			PreserveMessagePopup2.Open(title, stringDirect, stringDirect2, PreserveMessagePopup2.EPriceType.Gem, valueInt, delegate
			{
				RechargeStamina(onSuccess);
			});
		}
		else if (isNotEnough)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.IsKeyTypeDungeon(staminaType))
			{
				ResolveNotEnoughKey(staminaType, onSuccess, reqChapterIndex);
			}
			else if (!isSkip)
			{
				ResolveNotEnoughStaminaConfirm(staminaType, onSuccess, reqChapterIndex);
			}
			else
			{
				ResolveNotEnoughStamina(staminaType, onSuccess, reqChapterIndex);
			}
		}
	}

	private static string GetPotionEffectContent(ItemData<PotionItemData> potionItemData, string staminaName, int count)
	{
		if (potionItemData == null || count == 0)
		{
			return string.Empty;
		}
		string name = potionItemData.Name;
		int actionValue = potionItemData.SubData.ActionValue;
		long num = actionValue;
		int num2 = 0;
		long num3 = num2;
		string text;
		checked
		{
			if (potionItemData.SubData.ActionType == ItemActionType.RecoverStaminaPercent)
			{
				long maxStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(StaminaType.Chicken);
				long num4 = num * maxStamina;
				int num5 = 100;
				num3 = unchecked(num4 / num5) * count;
			}
			else
			{
				num3 = num * count;
			}
			text = LocalizationManager.GetString(ItemUse.USE_STAMINA_ITEM_CONFIRM, name, count, staminaName, num3);
		}
		if (potionItemData.SubData.BoosterCodes != null && potionItemData.SubData.BoosterCodes.Length != 0)
		{
			for (int i = 0; i < potionItemData.SubData.BoosterCodes.Length; i++)
			{
				string key = potionItemData.SubData.BoosterCodes[i];
				BoosterItemData data = GameRoot.Instance.BoosterItemDataContainer.GetData(key);
				if (data != null)
				{
					int num6 = data.Duration * count / 60;
					string boosterBonusTypeString = GetBoosterBonusTypeString(data.Type);
					text = text + "\n" + LocalizationManager.GetString(ItemUse.BOOSTER_EFFECT_APPLY, num6, boosterBonusTypeString, data.Value);
				}
			}
		}
		if (potionItemData.SubData.StaminaResetWarnningType != ResetType.None)
		{
			string text2 = string.Empty;
			if (potionItemData.SubData.StaminaResetWarnningType == ResetType.Daily)
			{
				text2 = LocalizationManager.GetString(NShared.NLocalization.System.NEXT_DAY_TIME, potionItemData.SubData.StaminaResetWarnningValue);
			}
			else if (potionItemData.SubData.StaminaResetWarnningType == ResetType.Weekly)
			{
				string text3 = LocalizationManager.GetString(NGame2.NUI.Common.GetDayOfWeekKey((DayOfWeek)potionItemData.SubData.StaminaResetWarnningValue));
				text2 = LocalizationManager.GetString(NShared.NLocalization.System.NEXT_WEEK_DAY, text3);
			}
			else if (potionItemData.SubData.StaminaResetWarnningType == ResetType.Monthly)
			{
				text2 = LocalizationManager.GetString(NShared.NLocalization.System.NEXT_MONTH_DAY, potionItemData.SubData.StaminaResetWarnningValue);
			}
			string stringDirect = LocalizationManager.GetStringDirect("USE_STAMINA_ITEM_CONFIRM_RESET_WARNING", staminaName, text2);
			text = text + "\n" + stringDirect;
		}
		return text.Replace("\\n", "\n");
	}

	private static string GetBoosterBonusTypeString(EventBonusType BonusType)
	{
		string result = string.Empty;
		if (BonusType == EventBonusType.BonusGold)
		{
			result = LocalizationManager.GetString(BoosterBonusType.BONUSTYPE_GOLD);
		}
		if (BonusType == EventBonusType.BonusExp)
		{
			result = LocalizationManager.GetString(BoosterBonusType.BONUSTYPE_EXP);
		}
		if (BonusType == EventBonusType.BonusItem)
		{
			result = LocalizationManager.GetString(BoosterBonusType.BONUSTYPE_ITEM);
		}
		if (BonusType == EventBonusType.SaveStamina)
		{
			result = LocalizationManager.GetString(BoosterBonusType.BONUSTYPE_SAVESTAMINA);
		}
		return result;
	}

	private static void CheckAndMoveCraftMenu(StaminaType staminaType)
	{
		bool flag = true;
		List<ItemData> potion = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetStaminaPotion(staminaType);
		int num = 0;
		if (potion != null && potion.Count > 0)
		{
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(potion[0].Index);
			if (itemInfo != null)
			{
				num = itemInfo.Count;
			}
			foreach (CraftItemData categoryDatum in GameRoot.Instance.CraftItemDataContainer.GetCategoryData(potion[0].Code))
			{
				foreach (CraftItemData.MaterialItemInfo materialItem in categoryDatum.GetMaterialItemList())
				{
					if (materialItem != null && GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(materialItem.ItemCode) < materialItem.ItemCount)
					{
						flag = false;
						break;
					}
				}
			}
		}
		if (flag && num <= 0)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string name = potion[0].Name;
			string stringDirect = LocalizationManager.GetStringDirect("INVENTORY.INVENTORY_LACK", name);
			string stringDirect2 = LocalizationManager.GetStringDirect("MOVE_BAG_CRAFT_MESSAGE_1");
			string message = $"{stringDirect}\n\n{stringDirect2}";
			MessagePopup.Open(title, message, delegate
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.HasEmptyCraftSlot())
				{
					SceneAutoBehaviour<InventoryManagement>.instance.OpenShortCutCraftWindow(potion[0], 0);
				}
				else
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("CRAFT_ITEMCOMPONENT_UNAVAILABLE"));
				}
			}, null, Inventory.INVENTORY_CRAFT_MOVE_MSG, NShared.NLocalization.Common.COMMON_CANCEL);
		}
		else
		{
			ShowRechargeImpossibleMessage(staminaType);
		}
	}

	private static bool ShowPopupNoMoreRechargeStamina(StaminaType staminaType)
	{
		if (staminaType != StaminaType.PrisonKey && staminaType != StaminaType.TechnoEnchantKey)
		{
			return false;
		}
		StaminaInfo staminaInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetStaminaInfo(staminaType);
		if (staminaInfo == null)
		{
			return false;
		}
		if (GetMaxRechargeableCount(staminaType) <= staminaInfo.RechargeCount)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(NShared.RechargeStamina.ResultType.ResetLimitExceeded);
			if (staminaType == StaminaType.TechnoEnchantKey)
			{
				message = LocalizationManager.GetStringDirect("GODKING_TRIAL.NOT_ENTER");
			}
			MessagePopup.Open(title, message);
			return true;
		}
		return false;
	}

	private static int GetMaxRechargeableCount(StaminaType staminaType)
	{
		StaminaInfo staminaInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetStaminaInfo(staminaType);
		if (staminaInfo == null)
		{
			return 0;
		}
		StaminaData staminaData = staminaInfo.StaminaData;
		if (staminaData == null)
		{
			return 0;
		}
		int num = 0;
		if (staminaData.RechargeGoldValue != null && staminaData.RechargeGoldValue.Length != 0 && int.Parse(staminaData.RechargeGoldValue[0]) > 0)
		{
			num = staminaData.RechargeGoldValue.Length;
		}
		if (num <= 0 && staminaData.RechargeGemValue != null && staminaData.RechargeGemValue.Length != 0 && int.Parse(staminaData.RechargeGemValue[0]) > 0)
		{
			num = staminaData.RechargeGemValue.Length;
		}
		return num;
	}

	private static void ResolveNotEnoughStaminaConfirm(StaminaType staminaType, Action onSuccess = null, int reqChapterIndex = 0)
	{
		if (ShowPopupNoMoreRechargeStamina(staminaType))
		{
			return;
		}
		long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(staminaType, reqChapterIndex);
		int num = 0;
		if (currentStamina > num)
		{
			return;
		}
		int rechargeStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetRechargeStamina(staminaType);
		int rechargeGold = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetRechargeGold(staminaType);
		int rechargeGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetRechargeGem(staminaType);
		if (rechargeGold == 0 && rechargeGem == 0)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.IsRegenType(staminaType))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("CONTENTS_ENTER_IMPOSSIBLE_REGEN"));
			}
			else
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("CONTENTS_ENTER_IMPOSSIBLE"));
			}
			return;
		}
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(staminaType);
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect(data.PopupStringKey, rechargeStamina);
		if (staminaType == StaminaType.TechnoEnchantKey)
		{
			int maxRechargeableCount = GetMaxRechargeableCount(staminaType);
			int num2 = 0;
			StaminaInfo staminaInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetStaminaInfo(staminaType);
			if (staminaInfo != null)
			{
				num2 = staminaInfo.RechargeCount;
			}
			stringDirect = LocalizationManager.GetStringDirect(data.PopupStringKey, rechargeStamina, maxRechargeableCount - num2);
		}
		string stringDirect2 = LocalizationManager.GetStringDirect("RECHARGE_ARENATICKET");
		bool useUnderLabel = data.RechargeGemValue.Length > 1;
		PreserveMessagePopup2.EPriceType priceType = ((rechargeGem <= 0) ? PreserveMessagePopup2.EPriceType.Gold : PreserveMessagePopup2.EPriceType.Gem);
		PreserveMessagePopup2.ForcedOpen(title, stringDirect, stringDirect2, priceType, rechargeGem, delegate
		{
			RechargeKey(staminaType, onSuccess);
		}, 0, null, useUnderLabel);
	}

	private static void ResolveNotEnoughStamina(StaminaType staminaType, Action onSuccess = null, int reqChapterIndex = 0)
	{
		if (!GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.IsRechargeable(staminaType))
		{
			if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2)
			{
				ShowRechargeImpossibleMessage(staminaType);
			}
			else if (staminaType == StaminaType.TreasureHouseKey)
			{
				CheckAndMoveCraftMenu(staminaType);
			}
			else
			{
				ShowRechargeImpossibleMessage(staminaType);
			}
		}
		else
		{
			if (ShowPopupNoMoreRechargeStamina(staminaType))
			{
				return;
			}
			long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(staminaType, reqChapterIndex);
			int num = 0;
			if (currentStamina > num)
			{
				return;
			}
			int num2;
			int num3;
			int num4;
			if (staminaType == StaminaType.HideoutKey)
			{
				num2 = GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.GetChapterMaxCount();
				num3 = 0;
				num4 = GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.GetCurrentChapterRechargeGem(reqChapterIndex);
			}
			else if (staminaType == StaminaType.ConquestKey)
			{
				num2 = GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetChapterMaxCount(staminaType);
				num3 = 0;
				num4 = GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetCurrentChapterRechargeGem(staminaType, reqChapterIndex);
			}
			else
			{
				num2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetRechargeStamina(staminaType);
				num3 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetRechargeGold(staminaType);
				num4 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetRechargeGem(staminaType);
			}
			if (num3 == 0 && num4 == 0)
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.IsRegenType(staminaType))
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("CONTENTS_ENTER_IMPOSSIBLE_REGEN"));
				}
				else
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("CONTENTS_ENTER_IMPOSSIBLE"));
				}
				return;
			}
			StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(staminaType);
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect(data.PopupStringKey, num2);
			if (staminaType == StaminaType.TechnoEnchantKey)
			{
				int maxRechargeableCount = GetMaxRechargeableCount(staminaType);
				int num5 = 0;
				StaminaInfo staminaInfo = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetStaminaInfo(staminaType);
				if (staminaInfo != null)
				{
					num5 = staminaInfo.RechargeCount;
				}
				stringDirect = LocalizationManager.GetStringDirect(data.PopupStringKey, num2, maxRechargeableCount - num5);
			}
			string stringDirect2 = LocalizationManager.GetStringDirect("RECHARGE_ARENATICKET");
			bool useUnderLabel = data.RechargeGemValue.Length > 1 || staminaType == StaminaType.HideoutKey;
			PreserveMessagePopup2.EPriceType priceType = ((num4 <= 0) ? PreserveMessagePopup2.EPriceType.Gold : PreserveMessagePopup2.EPriceType.Gem);
			PreserveMessagePopup2.Open(title, stringDirect, stringDirect2, priceType, num4, delegate
			{
				if (staminaType == StaminaType.HideoutKey)
				{
					RechargeHideDungeonKey((reqChapterIndex != 0) ? reqChapterIndex : NGame2.NAccount.UserCampaignManager.ChapterIndex, onSuccess);
				}
				else
				{
					RechargeKey(staminaType, onSuccess);
				}
			}, 0, delegate
			{
				if (staminaType == StaminaType.PrisonKey || staminaType == StaminaType.TechnoEnchantKey)
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("CONTENTS_ENTER_IMPOSSIBLE_REGEN"));
				}
			}, useUnderLabel);
		}
	}

	private static void ResolveNotEnoughKey(StaminaType staminaType, Action onSuccess = null, int reqChapterIndex = 0)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetChapterRemainCount(staminaType, reqChapterIndex) > 0)
		{
			return;
		}
		int chapterMaxCount = GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetChapterMaxCount(staminaType);
		int currentChapterRechargeGem = GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetCurrentChapterRechargeGem(staminaType, reqChapterIndex);
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("UPPERDUNGEON_RESET_MESSAGE1", chapterMaxCount);
		string stringDirect2 = LocalizationManager.GetStringDirect("RECHARGE_ARENATICKET");
		PreserveMessagePopup2.EPriceType priceType = PreserveMessagePopup2.EPriceType.Gem;
		PreserveMessagePopup2.Open(title, stringDirect, stringDirect2, priceType, currentChapterRechargeGem, delegate
		{
			if (staminaType == StaminaType.ConquestKey)
			{
				RechargeConquestDungeonKey((reqChapterIndex != 0) ? reqChapterIndex : NGame2.NAccount.UserCampaignManager.ChapterIndex, onSuccess);
			}
		}, 0, null, useUnderLabel: true);
	}

	private static void ShowRechargeImpossibleMessage(StaminaType staminaType)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.IsRegenType(staminaType) || GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetStaminaUpdateType(staminaType) == StaminaUpdateType.None)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("CONTENTS_ENTER_IMPOSSIBLE_REGEN"));
		}
		else
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("CONTENTS_ENTER_IMPOSSIBLE"));
		}
	}

	public static bool IsEnoughStamina(StaminaType staminaType, int staminaCount, int chapterIndex)
	{
		int num = 0;
		long num2 = num;
		switch (staminaType)
		{
		case StaminaType.HideoutKey:
		{
			int chapterRemainCount2 = GlobalAutoBehaviour<AccountManager>.instance.HideoutDungeonManager.GetChapterRemainCount(chapterIndex);
			num2 = chapterRemainCount2;
			break;
		}
		case StaminaType.ConquestKey:
		{
			int chapterRemainCount = GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetChapterRemainCount(staminaType, chapterIndex);
			num2 = chapterRemainCount;
			break;
		}
		default:
			num2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(staminaType);
			break;
		}
		long num3 = num2;
		return staminaCount <= num3;
	}

	public static bool CheckStamina(StaminaType staminaType, int staminaCount, int chapterIndex, Action onSuccess)
	{
		if (staminaType == StaminaType.GuildRaidTicket)
		{
			NShared.GuildRaidDungeonInfo guildRaidDungeonInfo = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance.GetGuildRaidDungeonInfo(chapterIndex) : null);
			if (guildRaidDungeonInfo != null && guildRaidDungeonInfo.FinishMode != 0)
			{
				return true;
			}
		}
		if (!IsEnoughStamina(staminaType, staminaCount, chapterIndex))
		{
			if (GameRoot.Instance.PotionItemDataContainer.CanRecoverStaminaWithItem(staminaType))
			{
				ResolveRecoverStamina(isNotEnough: true, null, staminaType);
			}
			else
			{
				ResolveRecoverStamina(isNotEnough: true, null, staminaType, chapterIndex, onSuccess);
			}
			return false;
		}
		return true;
	}

	public static DateTime GetClientTime(string TimeStr)
	{
		if (String_Extension_Method.IsNullOrEmpty(TimeStr))
		{
			return DateTime.MinValue;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(TimeStr, out var o_dateTime))
		{
			return DateTime.MinValue;
		}
		return GetClientTime(o_dateTime);
	}

	public static DateTime GetClientTime(DateTime dt)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(dt);
	}

	public static bool IsEnableDate(string beginTimeStr, string endTimeStr, bool isUTC = false)
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(beginTimeStr, out var o_dateTime))
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(endTimeStr, out var o_dateTime2))
		{
			return false;
		}
		return IsEnableDate(o_dateTime, o_dateTime2, isUTC);
	}

	public static bool IsEnableDate(DateTime beginTime, DateTime endTime, bool isUTC = false)
	{
		DateTime dateTime = ((!isUTC) ? GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime() : GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime());
		if (beginTime <= dateTime)
		{
			return dateTime <= endTime;
		}
		return false;
	}

	public static string GetFitEllipsisString(string str, UILabel label, int dotCount = 2)
	{
		return GetFitEllipsisString(str, label.width, label, label.fontSize, dotCount);
	}

	public static string GetFitEllipsisString(string str, int width, UILabel label, int fontSize, int dotCount = 2)
	{
		label.UpdateNGUIText(useValidFontSize: true);
		NGUIText.regionWidth = width;
		if (NGUIText.CalculatePrintedSize(str).y <= (float)fontSize)
		{
			return str;
		}
		string text = NGUIText.StripSymbols(str);
		int num = 0;
		int length = text.Length;
		int index = (num + length) / 2;
		string text2 = GetFitEllipsisString_BinarySearch(text, index, num, length, fontSize, dotCount);
		if (str.Length != text.Length)
		{
			int num2 = 0;
			int num3 = 0;
			int length2 = text2.Length;
			while (num2 < length2 && num3 < str.Length)
			{
				char c = str[num3];
				char c2 = text2[num2];
				if (c == c2)
				{
					num2++;
					num3++;
					continue;
				}
				if (c != '[')
				{
					break;
				}
				int sub = 0;
				bool bold = false;
				bool italic = false;
				bool underline = false;
				bool strike = false;
				bool ignoreColor = false;
				int index2 = num3;
				if (NGUIText.ParseSymbol(str, ref index2, null, premultiply: false, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
				{
					string text3 = str.Substring(num3, index2 - num3);
					num3 = index2;
					text2 = text2.Insert(num2, text3);
					num2 += text3.Length;
					length2 = text2.Length;
				}
				else
				{
					num2++;
					num3++;
				}
			}
		}
		return text2;
	}

	private static string MakeEllipsis(string text, int len, int dotCount)
	{
		string text2 = string.Empty;
		if (dotCount <= 0)
		{
			dotCount = 2;
		}
		for (int i = 0; i < dotCount; i++)
		{
			text2 += ".";
		}
		return text.Substring(0, len) + text2;
	}

	public static string GetFitEllipsisString_BinarySearch(string str, int index, int frontIndex, int rearIndex, int fontSize, int dotCount)
	{
		string text = MakeEllipsis(str, index, dotCount);
		if (NGUIText.CalculatePrintedSize(text).y > (float)fontSize)
		{
			if (index <= frontIndex)
			{
				return text;
			}
			string text2 = MakeEllipsis(str, index - 1, dotCount);
			if (NGUIText.CalculatePrintedSize(text2).y <= (float)fontSize)
			{
				return text2;
			}
			int index2 = (frontIndex + index) / 2;
			return GetFitEllipsisString_BinarySearch(str, index2, frontIndex, index, fontSize, dotCount);
		}
		if (index >= rearIndex)
		{
			return text;
		}
		if (NGUIText.CalculatePrintedSize(MakeEllipsis(str, index + 1, dotCount)).y > (float)fontSize)
		{
			return text;
		}
		int index3 = (rearIndex + index) / 2;
		return GetFitEllipsisString_BinarySearch(str, index3, index, rearIndex, fontSize, dotCount);
	}

	public static void SetProgressValue(UISprite progressSprite, UILabel valueLabel, long value, long max, bool showMaxValue = true)
	{
		float fillAmount = 0f;
		if (max != 0L)
		{
			fillAmount = (float)value / (float)max;
		}
		SetSpriteFillAmount(progressSprite, fillAmount);
		if (showMaxValue)
		{
			SetLabelText(valueLabel, $"{value:#,0} / {max:0,#}");
		}
		else
		{
			SetLabelText(valueLabel, $"{value:#,0}");
		}
	}

	public static void EnableToggle(UIToggle toggle, bool enable, bool grayscale = false)
	{
		if (toggle == null)
		{
			return;
		}
		toggle.enabled = enable;
		if (!grayscale)
		{
			return;
		}
		UIWidget component = toggle.GetComponent<UIWidget>();
		if ((bool)component)
		{
			bool flag = !enable;
			bool num = component.isGrayscale != flag;
			component.isGrayscale = flag;
			if (num && component.gameObject.activeSelf)
			{
				EnableGameObject(component, enable: false);
				EnableGameObject(component, enable: true);
			}
		}
	}

	public static void EnableCollider(Component component, bool isEnable)
	{
		if (!(component == null))
		{
			Collider component2 = component.GetComponent<Collider>();
			if (component2 != null)
			{
				component2.enabled = isEnable;
			}
		}
	}

	public static void EnableCollidersInChildren(Component component, bool isEnable)
	{
		if (component == null)
		{
			return;
		}
		Collider[] componentsInChildren = component.GetComponentsInChildren<Collider>();
		if (componentsInChildren != null)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = isEnable;
			}
		}
	}

	public static void OpenPTSDisabledPopup()
	{
		string stringDirect = LocalizationManager.GetStringDirect("INFORM");
		string stringDirect2 = LocalizationManager.GetStringDirect("PTS_UN_SUPPORTED");
		MessagePopup.Open(stringDirect, stringDirect2);
	}

	public static void OpenHeroAwakeChallengeItemPopup(int rewardIndex, Action onConfirm = null)
	{
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		if (__Extension_Method_ICollection__.COUNT(rewardItemList) == 0)
		{
			VespaDebug.LogError("reward nothing");
			return;
		}
		string message = LocalizationManager.GetString(ItemUse.ALLOWNEDREWARD_CONFIRM);
		string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
		IconConfirmPopup iconConfirmPopup = IconConfirmPopup.Open(message, confirmLabel, cancelLabel, onConfirm);
		for (int i = 0; i < rewardItemList.Count(); i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			if (rewardElementData != null)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
				if (dataByCode != null)
				{
					iconConfirmPopup.AddItem(dataByCode.Index, rewardElementData.ItemMin);
				}
				continue;
			}
			break;
		}
	}

	public static void OpenHeroAwakeChallengeItemPopup(int heroIndex, int heroStar, int heroTranscend, Action onConfirm = null)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			VespaDebug.LogError("CreatureData not found : " + heroIndex);
			return;
		}
		CreatureAwakeChallengeItemData data2 = GameRoot.Instance.CreatureAwakeChallengeItemDataContainer.GetData(data.TagType);
		if (data2 == null)
		{
			VespaDebug.LogError("No fragment item. HeroIndex=" + heroIndex + " TagType=" + data.TagType);
			return;
		}
		CreatureStarData data3 = GameRoot.Instance.CreatureStarDataContainer.GetData(heroStar, heroTranscend);
		int count = data3?.ExchangeItemCount ?? 0;
		string message = LocalizationManager.GetString(ItemUse.HEROCHOICE_FRAGMENT);
		string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
		IconConfirmPopup iconConfirmPopup = IconConfirmPopup.Open(message, confirmLabel, cancelLabel, onConfirm);
		int num = 1;
		while (true)
		{
			string itemCode = data2.GetItemCode(num);
			if (String_Extension_Method.IsNullOrEmpty(itemCode))
			{
				break;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
			if (dataByCode != null)
			{
				iconConfirmPopup.AddItem(dataByCode.Index, count);
			}
			num++;
		}
		if (heroTranscend > 0)
		{
			iconConfirmPopup.AddItem(data3.HeroTranscendItemIndex, data3.ExchangeTranscendedItemCount);
		}
	}

	public static string PartyDungeonTypeLocale(PartyDungeonType type)
	{
		return type switch
		{
			PartyDungeonType.None => LocalizationManager.GetStringDirect("PARTY.FILTER_DUNGEON_TYPE_ALL"), 
			PartyDungeonType.Campaign => LocalizationManager.GetStringDirect("CONTENT_CHAPTER"), 
			PartyDungeonType.Conquest => LocalizationManager.GetStringDirect("NODE_HUNTING_NAME"), 
			PartyDungeonType.Hideout => LocalizationManager.GetStringDirect("LOCATION_HIDEOUT_NAME"), 
			PartyDungeonType.EventDungeon => LocalizationManager.GetStringDirect("CONTENT_EVENT"), 
			_ => string.Empty, 
		};
	}

	public static string PartyChapterLocale(PartyDungeonChapter chapterType)
	{
		if (chapterType == PartyDungeonChapter.None)
		{
			return LocalizationManager.GetStringDirect("PARTY.FILTER_CHAPTER_ALL");
		}
		return LocalizationManager.GetStringDirect("CONTENT_" + chapterType.ToString().ToUpper());
	}

	public static string PartyStageLocale(int stage)
	{
		if (stage == 0)
		{
			return LocalizationManager.GetStringDirect("PARTY.FILTER_STAGE_ALL");
		}
		return stage.ToString();
	}

	public static string GetNodeName(int chapterIndex, int dungeonIndex)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("nodeData is null / chapterIndex : " + chapterIndex + " , dungeonIndex : " + dungeonIndex);
			return string.Empty;
		}
		string empty = string.Empty;
		string[] code = data.Code;
		if (code == null || code.Length == 0)
		{
			return $"{chapterIndex} - {dungeonIndex}";
		}
		return ArgParser.GetValueString(code, 0);
	}

	public static PartyDungeonDifficultyType GetPartyDungeonDifficultyType(NShared.DungeonDifficultyType dungeonDifficultyType)
	{
		return EnumAttributeContainer<NShared.DungeonDifficultyType, DungeonDifficultyTypeAttribute>.Get(dungeonDifficultyType)?.PartyDungeonDifficultyType ?? PartyDungeonDifficultyType.None;
	}

	public static NShared.DungeonDifficultyType GetDungeonDifficultyType(PartyDungeonDifficultyType partyDungeonDifficultyType)
	{
		return EnumAttributeContainer<PartyDungeonDifficultyType, PartyDungeonDifficultyTypeAttribute>.Get(partyDungeonDifficultyType)?.DungeonDifficultyType ?? NShared.DungeonDifficultyType.Easy;
	}

	public static string GetNodeDungeonName(CampaignNodeData nodeData)
	{
		if (nodeData == null)
		{
			return string.Empty;
		}
		return nodeData.Name;
	}

	public static string TextInsertWrapCharacter(string originString, string compareString)
	{
		if (String_Extension_Method.IsNullOrEmpty(originString) || String_Extension_Method.IsNullOrEmpty(compareString))
		{
			return originString;
		}
		int num = originString.IndexOf(compareString);
		if (num < 0)
		{
			return originString;
		}
		int num2 = num + compareString.Length;
		if (num2 >= originString.Length)
		{
			return originString;
		}
		return originString.Insert(num2, "\n");
	}

	public static string DungeonDifficultyLocale(NShared.DungeonDifficultyType type)
	{
		return type switch
		{
			NShared.DungeonDifficultyType.Easy => LocalizationManager.GetStringDirect("CHAPTERCLEAR_EASY"), 
			NShared.DungeonDifficultyType.Normal => LocalizationManager.GetStringDirect("CHAPTERCLEAR_NOMAL"), 
			NShared.DungeonDifficultyType.Hard => LocalizationManager.GetStringDirect("CHAPTERCLEAR_HARD"), 
			NShared.DungeonDifficultyType.Hell => LocalizationManager.GetStringDirect("CHAPTERCLEAR_HELL"), 
			_ => string.Empty, 
		};
	}

	public static string GradeLocaleText(byte grade)
	{
		if (grade == 0)
		{
			return LocalizationManager.GetStringDirect("BROADCAST.BROADCAST_TAG_ALL");
		}
		return LocalizationManager.GetStringDirect("ITEM_GRADE_" + grade);
	}

	public static CurrencyType GetCurrencyType(RewardType rewardType)
	{
		return EnumAttributeContainer<RewardType, RewardTypeAttribute>.Get(rewardType)?.CurrencyType ?? CurrencyType.None;
	}

	public static float CalculateTableHeight(UITable table)
	{
		Bounds bounds = default(Bounds);
		List<Transform> childList = table.GetChildList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(childList); i++)
		{
			Transform content = childList[i];
			Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(table.transform, content, considerInactive: true);
			bounds.Encapsulate(bounds2);
		}
		return bounds.size.y;
	}

	public static void TableCalcBounds(UITable table, UIWidget root, bool isFixedWidth = false, int customHeightPadding = 0)
	{
		Bounds bounds = default(Bounds);
		List<Transform> childList = table.GetChildList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(childList); i++)
		{
			Transform content = childList[i];
			Bounds bounds2 = NGUIMath.CalculateRelativeWidgetBounds(root.transform, content, considerInactive: true);
			bounds.Encapsulate(bounds2);
		}
		if (!isFixedWidth)
		{
			root.width = (int)bounds.size.x;
		}
		root.height = (int)bounds.size.y + __Extension_Method_ICollection__.COUNT(childList) * (int)table.padding.y + customHeightPadding;
		root.ForceUpdateAnchors(includeChildren: true);
	}

	public static CurrencyTooltipType GetCurrencyTooltipType(CurrencyType currencyType)
	{
		return EnumAttributeContainer<CurrencyType, CurrencyTypeAttribute>.Get(currencyType)?.TooltipType ?? CurrencyTooltipType.Default;
	}

	public static void CreatreNpcBuffIconComponent(NPCData npcData, List<GeneralIconComponent> componentContainer, UISimpleGrid buffIconGrid)
	{
		if (npcData == null || componentContainer == null || buffIconGrid == null)
		{
			return;
		}
		int num = 1;
		while (true)
		{
			EntryMenuBuffIconInfo buffIconInfo = npcData.GetBuffIconInfo(num);
			if (buffIconInfo == null || String_Extension_Method.IsNullOrEmpty(buffIconInfo.BuffIconName))
			{
				break;
			}
			if (String_Extension_Method.IsNullOrEmpty(buffIconInfo.BuffIconCondition) || NGame2.NUI.NWindow.EntryMenu.CheckEntryMenuCondition(null, buffIconInfo.BuffIconCondition))
			{
				string attrTypeIconPath = GetAttrTypeIconPath(buffIconInfo.BuffIconName);
				GeneralIconComponent generalIconComponent = buffIconGrid.AddGridItem(GeneralIconComponent.PrefabAuto);
				if (buffIconInfo.BuffIconCondition == typeof(GuildActivityRestrict).Name)
				{
					IGuildPenalty validGuildPenalty = GuildManager.GetValidGuildPenalty();
					generalIconComponent.OpenGuildContentsRestrict(attrTypeIconPath, validGuildPenalty.EndTime(), validGuildPenalty.GetPenaltyType());
				}
				else
				{
					generalIconComponent.Open(attrTypeIconPath, null, GeneralIconComponent.DecoType.EntryMenuBuff);
				}
				componentContainer.Add(generalIconComponent);
			}
			num++;
		}
	}

	public static void CloseWindowsBeforeRaidPartySetting()
	{
		if (SceneAutoBehaviour<MyHeroManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<InventoryManagement>.isValidInstance)
		{
			SceneAutoBehaviour<InventoryManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<SubStoryManagement>.isValidInstance)
		{
			SceneAutoBehaviour<SubStoryManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<ShopManagement>.isValidInstance)
		{
			SceneAutoBehaviour<ShopManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<GuildManagement2>.isValidInstance)
		{
			SceneAutoBehaviour<GuildManagement2>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<EventForgeManagement>.isValidInstance)
		{
			SceneAutoBehaviour<EventForgeManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<RaidManagement>.isValidInstance)
		{
			AffixSelectPopup instance = SceneAutoBehaviour<RaidManagement>.instance.GetInstance<AffixSelectPopup>(openedOnly: true);
			if (instance != null && instance.IsOpen())
			{
				instance.Close();
			}
		}
	}

	public static bool CheckEventDungeonOpenCondition(EventDungeonData eventDungeonData, out string notOpenText)
	{
		notOpenText = "";
		bool flag = CheckEventDungeonOpenCondition(eventDungeonData, eventDungeonData.OpenType1, eventDungeonData.OpenTypeValues1, out notOpenText);
		if (flag)
		{
			flag = CheckEventDungeonOpenCondition(eventDungeonData, eventDungeonData.OpenType2, eventDungeonData.OpenTypeValues2, out notOpenText);
		}
		return flag;
	}

	private static bool CheckEventDungeonOpenCondition(EventDungeonData eventDungeonData, string openType, string[] values, out string notOpenText)
	{
		notOpenText = "";
		if (String_Extension_Method.IsNullOrEmpty(openType))
		{
			return true;
		}
		return SingletonAuto<EventDungeonCondtionContainer>.instance.GetCondition(openType)?.Check(eventDungeonData, values, out notOpenText) ?? true;
	}

	public static PartySlotComponent AddEmptySlotTemplate(Component grid, PartySlotComponent emptySlotTemplate)
	{
		if (grid == null || emptySlotTemplate == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(emptySlotTemplate.gameObject);
		if (gameObject == null)
		{
			return null;
		}
		gameObject.transform.parent = grid.transform;
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(gameObject.transform);
		gameObject.transform.localScale = emptySlotTemplate.transform.localScale;
		gameObject.transform.rotation = emptySlotTemplate.transform.rotation;
		EnableGameObject(gameObject, enable: true);
		return gameObject.GetComponent<PartySlotComponent>();
	}

	public static bool IsUsableOneTimeBoosterItem(CampaignDungeonData dungeonData, bool isUseOnetimeBooster, bool isScenario)
	{
		if (dungeonData == null)
		{
			return false;
		}
		if (!isUseOnetimeBooster || isScenario)
		{
			return false;
		}
		if (dungeonData.BattleType == BattleType.Tower || dungeonData.BattleType == BattleType.ChallengeTower || dungeonData.BattleType == BattleType.Maze)
		{
			int towerIndex = GameRoot.Instance.TowerFloorDataContainer.GetTowerIndex(dungeonData.ChapterIndex, dungeonData.DungeonIndex);
			if (towerIndex == -1)
			{
				return IsContainBoosterItemDataBattleTypes(dungeonData.BattleType);
			}
			TowerData towerData = GameRoot.Instance.TowerDataContainer.GetData(towerIndex);
			if (towerData == null)
			{
				return IsContainBoosterItemDataBattleTypes(dungeonData.BattleType);
			}
			TowerInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.GetValues();
			if (__Extension_Method_ICollection__.IsNullOrEmpty(values))
			{
				return false;
			}
			TowerInfo towerInfo = Array.Find(values, (TowerInfo e) => e.TowerIndex == towerData.TowerIndex);
			if (towerInfo == null)
			{
				return false;
			}
			if (dungeonData.DungeonIndex <= towerInfo.CompletedFloor)
			{
				return IsContainBoosterItemDataBattleTypes(dungeonData.BattleType);
			}
			return false;
		}
		return IsContainBoosterItemDataBattleTypes(dungeonData.BattleType);
	}

	public static bool IsContainBoosterItemDataBattleTypes(BattleType battleType)
	{
		return GetOnetimeBoosterItemData()?.BattleTypes.Contains(battleType) ?? false;
	}

	public static BoosterItemData GetOnetimeBoosterItemData()
	{
		IList<BoosterItemData> allData = GameRoot.Instance.BoosterItemDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return null;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			BoosterItemData boosterItemData = allData[i];
			if (boosterItemData != null && boosterItemData.IsOnetime)
			{
				return boosterItemData;
			}
		}
		return null;
	}

	public static bool IsEnableGachas(int[] gachaIndices)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(gachaIndices); i++)
		{
			if (IsEnableGacha(gachaIndices[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsEnableGacha(int gachaIndex)
	{
		NewEquipGachaData data = GameRoot.Instance.NewEquipGachaDataContainer.GetData(gachaIndex);
		if (data == null)
		{
			return false;
		}
		EquipGachaInfo equipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Get(gachaIndex);
		if (equipGachaInfo == null)
		{
			return false;
		}
		if (data.IsStarterPickupGacha())
		{
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.StarterGachaManager.IsRemainGachaCount(gachaIndex);
			if (HelperItem.GetFreeGachaItemCount(gachaIndex) <= 0)
			{
				if (!flag)
				{
					return false;
				}
				if (equipGachaInfo.Durational == 1 && !IsEnableDate(equipGachaInfo.BeginTime, equipGachaInfo.EndTime, isUTC: true))
				{
					return false;
				}
			}
		}
		else
		{
			if (!equipGachaInfo.OnSale)
			{
				return false;
			}
			if (equipGachaInfo.Durational == 1 && !IsEnableDate(equipGachaInfo.BeginTime, equipGachaInfo.EndTime, isUTC: true))
			{
				return false;
			}
			if (!SingletonAuto<ConditionChecker>.instance.Check(data.OpenCondition))
			{
				return false;
			}
		}
		return true;
	}

	public static void RefreshComponent(Component component)
	{
		EnableGameObject(component, enable: true, isForce: true);
	}

	public static void ShowBonusItemEventByMinMax(ItemIconComponent comp, int itemIndex, long minCount = 0, long maxCount = 0, BattleType battleType = BattleType.None)
	{
		long num = minCount;
		int num2 = 0;
		if (num <= num2)
		{
			return;
		}
		long num3 = maxCount;
		int num4 = 0;
		checked
		{
			if (num3 > num4)
			{
				DropBonusEvent bonusItemEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetBonusItemEvent(itemIndex, EventBonusType.BonusAssignedItem, battleType);
				if (bonusItemEvent != null)
				{
					minCount += GlobalAutoBehaviour<DropBonusEventManager>.instance.GetBonusAssignedItemCount(minCount, bonusItemEvent.BonusPercentage);
					maxCount += GlobalAutoBehaviour<DropBonusEventManager>.instance.GetBonusAssignedItemCount(maxCount, bonusItemEvent.BonusPercentage);
				}
				DropBonusEvent bonusItemEvent2 = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetBonusItemEvent(itemIndex, EventBonusType.BonusDungeonItemCount, battleType);
				if (bonusItemEvent2 != null)
				{
					long num5 = minCount;
					int bonusPercentage = bonusItemEvent2.BonusPercentage;
					minCount = num5 + bonusPercentage;
					long num6 = maxCount;
					int bonusPercentage2 = bonusItemEvent2.BonusPercentage;
					maxCount = num6 + bonusPercentage2;
				}
				if (bonusItemEvent == null && bonusItemEvent2 == null)
				{
					comp.SetItemMinMax(minCount, maxCount);
					return;
				}
				comp.SetItemMinMax(minCount, maxCount, Color.green);
				comp.ShowEventBonus(isShow: true);
			}
		}
	}

	public static void SetGridMaxPerLine(UIGrid grid, int maxPerLine)
	{
		if (!(grid == null))
		{
			grid.maxPerLine = maxPerLine;
		}
	}

	public static EquipItemInfo GetFakeSoulWeaponInfo(EquipItemInfo equipItemInfo, int grade = 0, int reinforceLevel = 0, int exp = 0, int optionRatio1 = 0, int optionRatio2 = 0, int optionBonusRatio1 = 0, int optionBonusRatio2 = 0)
	{
		if (equipItemInfo == null)
		{
			return null;
		}
		EquipItemInfo equipItemInfo2 = equipItemInfo.Clone();
		EquipItemData equipItemData = equipItemInfo2.EquipItemData;
		if (equipItemData == null)
		{
			return null;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(equipItemData.CreatureIndex))
		{
			return null;
		}
		int[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("FixedRatios");
		if (__Extension_Method_ICollection__.IsNullOrEmpty(valueArray) || __Extension_Method_ICollection__.COUNT(valueArray) < 2)
		{
			return null;
		}
		SoulWeaponInfo soulWeaponInfo = new SoulWeaponInfo
		{
			EquipItemSlotIndex = equipItemInfo2.SlotIndex,
			ItemIndex = equipItemInfo2.ItemIndex,
			Grade = grade,
			ReinforceLevel = reinforceLevel,
			Exp = exp,
			OptionRatio1 = ((optionRatio1 == 0) ? valueArray[0] : optionRatio1),
			OptionRatio2 = ((optionRatio2 == 0) ? valueArray[1] : optionRatio2),
			OptionBonusRatio1 = optionBonusRatio1,
			OptionBonusRatio2 = optionBonusRatio2
		};
		equipItemInfo2.SetSoulWeaponInfo(soulWeaponInfo);
		return equipItemInfo2;
	}

	public static void PlaySoulWeaponNodeTree(NodeTreeUIPlayer nodeTreePlayer, EquipItemInfo equipItemInfo, SoulWeaponNodeTreeType type, Action onStart, Action onEnd, bool isPlayOnce = true)
	{
		if (nodeTreePlayer == null || equipItemInfo == null)
		{
			return;
		}
		SoulWeaponNodeTreeData soulWeaponNodeTreeData = GetSoulWeaponNodeTreeData(equipItemInfo);
		if (soulWeaponNodeTreeData == null)
		{
			return;
		}
		string packCode = soulWeaponNodeTreeData.GetPackCode(type);
		string treeName = soulWeaponNodeTreeData.GetTreeName(type);
		if (!String_Extension_Method.IsNullOrEmpty(packCode) && !String_Extension_Method.IsNullOrEmpty(treeName))
		{
			int num = ((equipItemInfo.SoulWeaponInfo != null) ? equipItemInfo.SoulWeaponInfo.Grade : 0);
			nodeTreePlayer.AddNodeTreeKeyWords(VariableKeyword.SoulWeaponGrade, num);
			new StringKey();
			if (isPlayOnce)
			{
				nodeTreePlayer.PlayOnce(packCode, treeName, onStart, onEnd);
			}
			else
			{
				nodeTreePlayer.PlaySequence(packCode, treeName, onStart, onEnd);
			}
		}
	}

	private static SoulWeaponNodeTreeData GetSoulWeaponNodeTreeData(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null || !equipItemInfo.IsSoulWeapon())
		{
			return null;
		}
		ItemData itemData = equipItemInfo.ItemData;
		if (itemData == null)
		{
			return null;
		}
		string code = itemData.Code;
		SoulWeaponNodeTreeData data = GameRoot.Instance.SoulWeaponNodeTreeDataContainer.GetData(code);
		if (data == null)
		{
			return null;
		}
		return data;
	}

	public static int GetSoulWeaponEquipHeroIndex(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo == null)
		{
			return 0;
		}
		string itemCode = soulWeaponInfo.GetItemCode();
		if (String_Extension_Method.IsNullOrEmpty(itemCode))
		{
			return 0;
		}
		EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(itemCode);
		if (data == null || !data.IsOpenSoulWeapon)
		{
			return 0;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(data.CreatureIndex))
		{
			return 0;
		}
		return data.CreatureIndex[0];
	}

	public static bool IsOpenSoulWeaponLiberationButton()
	{
		if (!SoulWeaponManagement.IsOpenSoulweapon() || !SoulWeaponManagement.IsOpenGodKingTemple())
		{
			return false;
		}
		if (SoulWeaponManagement.IsCompleteSoulWeaponLiberation())
		{
			return true;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.HaveSoulWeapon())
		{
			return true;
		}
		EquipItemInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetValues();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(values); i++)
		{
			EquipItemInfo equipItemInfo = values[i];
			if (equipItemInfo == null || !equipItemInfo.IsUniqueWeapon)
			{
				continue;
			}
			EquipItemData equipItemData = equipItemInfo.EquipItemData;
			if (equipItemData != null && equipItemData.IsOpenSoulWeapon && GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.HaveSoulWeaponHero(equipItemInfo))
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.CheckAlreadySoulWeapon(equipItemInfo))
				{
					return true;
				}
				if (equipItemInfo.GetInventoryType() != InventoryType.Chest)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool ReleaseWidgetAnchor(Transform transform)
	{
		if (transform == null)
		{
			return false;
		}
		UIWidget component = transform.GetComponent<UIWidget>();
		if (component == null)
		{
			return false;
		}
		component.SetAnchor(null, 0, 0, 0, 0);
		return true;
	}

	public static string GetSupportTextureFormat()
	{
		List<TextureFormat> obj = new List<TextureFormat>
		{
			TextureFormat.ASTC_6x6,
			TextureFormat.ETC2_RGB
		};
		StringBuilder stringBuilder = new StringBuilder();
		foreach (TextureFormat item in obj)
		{
			if (SystemInfo.SupportsTextureFormat(item))
			{
				stringBuilder.Append($" {item.ToString()}");
			}
		}
		return stringBuilder.ToString();
	}

	public static bool IsATSCSupported()
	{
		return true;
	}

	public static void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	public static void CreateOrReleaseCompnent<T>(List<T> listComp, int makeCompCount, Transform trs, PrefabAuto<T> prefabAuto) where T : Component
	{
		if (listComp == null)
		{
			listComp = new List<T>();
		}
		int num = __Extension_Method_ICollection__.COUNT(listComp);
		int num2 = makeCompCount - num;
		if (num2 == 0)
		{
			return;
		}
		bool flag = num2 < 0;
		for (int num3 = (flag ? (-num2 - 1) : (num2 - 1)); num3 >= 0; num3--)
		{
			if (flag)
			{
				FactoryBase.ReleaseComponent(listComp[num3]);
				listComp.RemoveAt(num3);
			}
			else
			{
				T val = prefabAuto.Factory.Create(trs);
				if (val == null)
				{
					break;
				}
				listComp.Add(val);
			}
		}
	}

	public static void CreateOrReleaseCompnent<T>(List<T> listComp, int makeCompCount, IGrid grid, PrefabAuto<T> prefabAuto) where T : Component
	{
		if (listComp == null)
		{
			listComp = new List<T>();
		}
		int num = __Extension_Method_ICollection__.COUNT(listComp);
		int num2 = makeCompCount - num;
		if (num2 == 0)
		{
			return;
		}
		bool flag = num2 < 0;
		for (int num3 = (flag ? (-num2 - 1) : (num2 - 1)); num3 >= 0; num3--)
		{
			if (flag)
			{
				FactoryBase.ReleaseComponent(listComp[num3]);
				listComp.RemoveAt(num3);
			}
			else
			{
				T val = AddGridItem(grid, prefabAuto);
				if (val == null)
				{
					break;
				}
				listComp.Add(val);
			}
		}
	}

	public static PreserveMessagePopup2.EPriceType ConvertToPriceType(CurrencyType currency)
	{
		switch (currency)
		{
		case CurrencyType.Gem:
			return PreserveMessagePopup2.EPriceType.Gem;
		case CurrencyType.Gold:
			return PreserveMessagePopup2.EPriceType.Gold;
		case CurrencyType.Mileage:
			return PreserveMessagePopup2.EPriceType.Mileage;
		default:
			VespaDebug.LogError("Not defined priceType : " + currency);
			return PreserveMessagePopup2.EPriceType.None;
		}
	}

	private static bool IsSkipCurrencyConfirmPopup(CurrencyType currencyType, long reqCurrency)
	{
		if (currencyType != CurrencyType.Gem)
		{
			return false;
		}
		int rubyPopup_Skip_Count = SystemOptionData.RubyPopup_Skip_Count;
		int num = rubyPopup_Skip_Count;
		if (reqCurrency > num)
		{
			return false;
		}
		return true;
	}

	public static void BuyConfirmPopupOpen(CurrencyType currencyType, long currencyCount, Action OpenAction, Action SkipAction)
	{
		if (!IsSkipCurrencyConfirmPopup(currencyType, currencyCount))
		{
			Vespa_System_Action_Extension_Method.fire(OpenAction);
			return;
		}
		int num = 0;
		long num2 = num;
		if (currencyType == CurrencyType.Gem)
		{
			num2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		}
		if (num2 < currencyCount)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string text = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
			text += string.Format("\n" + LocalizationManager.GetStringDirect("RUBY_POPUP_OPTION_SHORTAGE", currencyType));
			MessagePopup.Open(title, text, delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
			}, null);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(SkipAction);
		}
	}
}
