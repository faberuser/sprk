using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NDropBonusEvent;
using NGame2.NGuild;
using NGame2.NPet;
using NGame2.NUI.NManager.ClassBuff;
using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class BoosterManager : GlobalAutoBehaviour<BoosterManager>, IUpdater, IUserObserver<NGame2.NAccount.UserItemTimeDurationManager.BoosterChangeInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver<PlayerProductPurchaseInfo>, IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>, IUserObserver<ClassBuffInfo>
{
	private List<BoosterInfo> BoosterInfoList = new List<BoosterInfo>();

	private List<WeakRef<IBoosterListObserver>> _observerList = new List<WeakRef<IBoosterListObserver>>();

	private BattleType _battleType;

	private bool _includeCash;

	private DateTime _lastBoosterCheckTime = DateTime.UtcNow;

	private List<BoosterInfo> oldBoosterInfoList = new List<BoosterInfo>();

	private List<int> newBoosterItemIndexList = new List<int>();

	private Dictionary<EventBonusType, BoosterInfo> cachedLegendCostumeEventBonusDic = new Dictionary<EventBonusType, BoosterInfo>();

	private void Start()
	{
		GlobalUpdateBehaviour.AddUpdater(this);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.AddObserver(this);
	}

	public void BindAccount()
	{
		GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.PurchaseTimeDurationManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager.ObserverManager.AddObserver(this);
	}

	public static void GetBoosterTooltipString(BoosterItemData boosterItemData, out string name, out string desc)
	{
		desc = "";
		name = "";
		if (boosterItemData != null)
		{
			desc = LocalizationManager.GetStringDirect(boosterItemData.Desc);
			name = LocalizationManager.GetStringDirect(boosterItemData.Name);
		}
	}

	public void AddObserver(IBoosterListObserver observer)
	{
		if (!_observerList.Any((WeakRef<IBoosterListObserver> e) => e.Value == observer))
		{
			WeakRef<IBoosterListObserver> weakRef = new WeakRef<IBoosterListObserver>();
			weakRef.Value = observer;
			_observerList.Add(weakRef);
		}
	}

	public void RemoveObserver(IBoosterListObserver observer)
	{
		_observerList.RemoveAll((WeakRef<IBoosterListObserver> e) => e.Value == observer);
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}

	void IUserObserver<NGame2.NAccount.UserItemTimeDurationManager.BoosterChangeInfo>.OnChanged(NGame2.NAccount.UserItemTimeDurationManager.BoosterChangeInfo info)
	{
		if (info == null)
		{
			return;
		}
		List<PlayerItemTimeDurationInfo> itemTimeDurationInfos = info.ItemTimeDurationInfos;
		if (__Extension_Method_ICollection__.COUNT(itemTimeDurationInfos) > 0)
		{
			foreach (PlayerItemTimeDurationInfo item in itemTimeDurationInfos)
			{
				AddNewBoosterItemIndex(item.ItemIndex);
			}
		}
		NGame2.NAccount.UserItemTimeDurationManager.BoosterChangeType changeType = info.changeType;
		if ((uint)changeType <= 1u)
		{
			RefreshAllEffectiveBooster();
		}
	}

	void IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>.OnChanged(DropBonusEventManager.DropBonusEventChangeInfo info)
	{
		if (info != null)
		{
			DropBonusEventManager.DropBonusEventChangeType changeType = info.changeType;
			if ((uint)changeType <= 1u)
			{
				RefreshAllEffectiveBooster();
			}
		}
	}

	void IUserObserver<PlayerProductPurchaseInfo>.OnChanged(PlayerProductPurchaseInfo changedInfo)
	{
		RefreshAllEffectiveBooster();
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo != null && changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
		{
			RefreshAllEffectiveBooster();
		}
	}

	void IUpdater.ChainUpdate()
	{
		UpdateBooster();
	}

	private void UpdateBooster()
	{
		if ((DateTime.UtcNow - _lastBoosterCheckTime).TotalSeconds < 1.0)
		{
			return;
		}
		_lastBoosterCheckTime = DateTime.UtcNow;
		bool flag = false;
		BoosterInfo expiredBooster = null;
		foreach (BoosterInfo boosterInfo in BoosterInfoList)
		{
			if (!boosterInfo.IsEffective())
			{
				flag = true;
				expiredBooster = boosterInfo;
				break;
			}
		}
		if (flag)
		{
			RefreshAllEffectiveBooster(expiredBooster);
		}
	}

	private List<BoosterInfo> GetAllEffectiveBoosterItem(BattleType battleType = BattleType.None)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		PlayerItemTimeDurationInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.GetValues();
		if (__Extension_Method_ICollection__.COUNT(values) > 0)
		{
			PlayerItemTimeDurationInfo[] array = values;
			for (int i = 0; i < array.Length; i++)
			{
				ItemBoosterInfo itemBoosterInfo = CreateBoosterInfo(array[i], battleType, _includeCash);
				if (itemBoosterInfo != null)
				{
					list.Add(itemBoosterInfo);
				}
			}
		}
		return list;
	}

	public static ItemBoosterInfo CreateBoosterInfo(PlayerItemTimeDurationInfo durationIfo, BattleType battleType = BattleType.None, bool includeCash = false)
	{
		if (durationIfo == null)
		{
			return null;
		}
		ItemData<BoosterItemData> data = GameRoot.Instance.ItemDataContainer.GetData<BoosterItemData>(durationIfo.ItemIndex);
		BoosterItemData boosterItemData = null;
		if (data == null)
		{
			boosterItemData = GameRoot.Instance.BoosterItemDataContainer.GetBoosterDataByGroupIndex(durationIfo.ItemIndex);
			if (boosterItemData == null)
			{
				return null;
			}
		}
		else
		{
			boosterItemData = GameRoot.Instance.BoosterItemDataContainer.GetData(data.Code);
			if (boosterItemData == null)
			{
				return null;
			}
		}
		if (battleType != BattleType.None && __Extension_Method_ICollection__.COUNT(boosterItemData.BattleTypes) > 0 && !boosterItemData.BattleTypes.Contains(battleType))
		{
			return null;
		}
		ItemData itemData = ((data != null) ? GameRoot.Instance.ItemDataContainer.GetData(boosterItemData.BoosterItemIndex) : null);
		if (boosterItemData.IsOnetime)
		{
			if (includeCash)
			{
				return null;
			}
			ItemBoosterInfo obj = new ItemBoosterInfo
			{
				Type = BoosterInfoType.BoosterItem,
				BattleTypes = boosterItemData.BattleTypes,
				ShowAtBattle = !boosterItemData.IsHideAtBattle,
				ItemCode = ((itemData != null) ? itemData.Code : boosterItemData.ItemCode),
				ItemIndex = (itemData?.Index ?? boosterItemData.BoosterItemIndex),
				BonusType = (data?.SubData.Type ?? boosterItemData.Type),
				Value = (data?.SubData.Value ?? boosterItemData.Value),
				IsOnetime = boosterItemData.IsOnetime,
				EndTimeUTC = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()
			};
			GetBoosterTooltipString((data != null) ? data.SubData : boosterItemData, out var text, out var desc);
			obj.Name = text;
			obj.Desc = desc;
			return obj;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(durationIfo.EndTime, out var o_dateTime))
		{
			return null;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if ((int)(o_dateTime - currentServerUTCTime).TotalSeconds <= 0)
		{
			return null;
		}
		ItemBoosterInfo obj2 = new ItemBoosterInfo
		{
			Type = BoosterInfoType.BoosterItem,
			BattleTypes = boosterItemData.BattleTypes,
			ShowAtBattle = !boosterItemData.IsHideAtBattle,
			ItemCode = ((itemData != null) ? itemData.Code : boosterItemData.ItemCode),
			ItemIndex = (itemData?.Index ?? boosterItemData.BoosterItemIndex),
			BonusType = (data?.SubData.Type ?? boosterItemData.Type),
			Value = (data?.SubData.Value ?? boosterItemData.Value),
			EndTimeUTC = o_dateTime
		};
		GetBoosterTooltipString((data != null) ? data.SubData : boosterItemData, out var text2, out var desc2);
		obj2.Name = text2;
		obj2.Desc = desc2;
		return obj2;
	}

	private List<BoosterInfo> GetAllEffectiveCash(BattleType battleType = BattleType.None)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		if (GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager == null || GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager == null)
		{
			return list;
		}
		PlayerProductPurchaseInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.GetValues();
		if (__Extension_Method_ICollection__.COUNT(values) > 0)
		{
			PlayerProductPurchaseInfo[] array = values;
			foreach (PlayerProductPurchaseInfo playerProductPurchaseInfo in array)
			{
				if (playerProductPurchaseInfo == null)
				{
					continue;
				}
				NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(playerProductPurchaseInfo.ProductIndex);
				if (newPayShopProductInfo != null && newPayShopProductInfo.ProductType == ProductType.Gem && Vespa_String_Extension_Method2.TryParseNetworkTime2(playerProductPurchaseInfo.EndTime, out var o_dateTime))
				{
					DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
					if (!(o_dateTime < currentServerUTCTime))
					{
						BoosterInfo boosterInfo = new BoosterInfo();
						boosterInfo.Type = BoosterInfoType.BoosterFixedTerm;
						boosterInfo.EndTimeUTC = o_dateTime;
						boosterInfo.ItemCode = "";
						boosterInfo.Name = LocalizationManager.GetStringDirect(newPayShopProductInfo.ProductName);
						boosterInfo.Desc = LocalizationManager.GetStringDirect(newPayShopProductInfo.ProductDesc);
						list.Add(boosterInfo);
					}
				}
			}
		}
		return list;
	}

	private List<BoosterInfo> GetAllEffectiveFixedTerm(BattleType battleType = BattleType.None)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		if (GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager == null)
		{
			return list;
		}
		PlayerProductPurchaseInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.GetValues();
		if (__Extension_Method_ICollection__.COUNT(values) > 0)
		{
			PlayerProductPurchaseInfo[] array = values;
			for (int i = 0; i < array.Length; i++)
			{
				BoosterInfo boosterInfo = CreateBoosterInfo(array[i], battleType);
				if (boosterInfo != null)
				{
					list.Add(boosterInfo);
				}
			}
		}
		return list;
	}

	public static BoosterInfo CreateBoosterInfo(PlayerProductPurchaseInfo info, BattleType battleType = BattleType.None)
	{
		if (info == null)
		{
			return null;
		}
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(info.ProductIndex);
		if (newPayShopProductInfo == null)
		{
			return null;
		}
		if (newPayShopProductInfo.ProductType == ProductType.Booster)
		{
			return null;
		}
		if (newPayShopProductInfo.IsPurchaseMarketing)
		{
			return null;
		}
		if (battleType != BattleType.None)
		{
			if (__Extension_Method_ICollection__.COUNT(newPayShopProductInfo.UseItemCode) <= 0)
			{
				return null;
			}
			string key = newPayShopProductInfo.UseItemCode[0];
			BoosterItemData data = GameRoot.Instance.BoosterItemDataContainer.GetData(key);
			if (data == null)
			{
				return null;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(data.BattleTypes))
			{
				return null;
			}
			if (!data.BattleTypes.Contains(battleType))
			{
				return null;
			}
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(info.EndTime, out var o_dateTime))
		{
			return null;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (o_dateTime < currentServerUTCTime)
		{
			return null;
		}
		BoosterInfo boosterInfo = new BoosterInfo();
		boosterInfo.Type = BoosterInfoType.BoosterFixedTerm;
		boosterInfo.EndTimeUTC = o_dateTime;
		if (newPayShopProductInfo.ProductType != ProductType.DailyItem)
		{
			boosterInfo.ItemCode = ((__Extension_Method_ICollection__.COUNT(newPayShopProductInfo.UseItemCode) > 0) ? newPayShopProductInfo.UseItemCode[0] : "");
		}
		else
		{
			boosterInfo.ItemCode = newPayShopProductInfo.ProductName;
		}
		boosterInfo.Name = LocalizationManager.GetStringDirect(newPayShopProductInfo.ProductName);
		boosterInfo.Desc = LocalizationManager.GetStringDirect(newPayShopProductInfo.ProductDesc);
		return boosterInfo;
	}

	private List<BoosterInfo> GetAllEffectiveGuildSkill(BattleType battleType = BattleType.None)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0L)
		{
			return list;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
		{
			GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
			if (myMemberInfo != null && myMemberInfo.Rank == 5)
			{
				return list;
			}
		}
		new List<Tuple<GuildSkillData, GuildSkillLevelData>>();
		GuildSkillLevelData[] guildSkillDataList = GlobalAutoBehaviour<GuildManager>.instance.GetGuildSkillDataList();
		if (guildSkillDataList == null)
		{
			return list;
		}
		GuildSkillLevelData[] array = guildSkillDataList;
		foreach (GuildSkillLevelData guildSkillLevelData in array)
		{
			GuildSkillData data = GameRoot.Instance.GuildSkillDataContainer.GetData(guildSkillLevelData.GuildSkillIndex);
			if (data != null && data.BonusType != EventBonusType.None)
			{
				GuildSkillBoosterInfo guildSkillBoosterInfo = new GuildSkillBoosterInfo();
				guildSkillBoosterInfo.ItemCode = data.Code;
				guildSkillBoosterInfo.BonusType = data.BonusType;
				guildSkillBoosterInfo.Value = guildSkillLevelData.Values[0];
				guildSkillBoosterInfo.Name = LocalizationManager.GetStringDirect(data.ToolTipName);
				guildSkillBoosterInfo.Desc = LocalizationManager.GetStringDirect(data.ToolTipDesc, guildSkillBoosterInfo.Value);
				list.Add(guildSkillBoosterInfo);
			}
		}
		return list;
	}

	public List<BoosterInfo> GetNormalCostumeBoosterInfoList(BattleType battleType = BattleType.None)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		Dictionary<string, int> dictionary = null;
		if (GlobalAutoBehaviour<AccountManager>.instance.CostumeManager != null)
		{
			foreach (CostumeInfo item in GlobalAutoBehaviour<AccountManager>.instance.CostumeManager)
			{
				int costumeIndex = item.CostumeIndex;
				CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
				if (data == null || data.CostumeType == EHeroCostumeType.Legend)
				{
					continue;
				}
				List<CostumeAbilityInfo> costumeAbilities = data.GetCostumeAbilities();
				if (__Extension_Method_ICollection__.COUNT(costumeAbilities) == 0)
				{
					continue;
				}
				foreach (CostumeAbilityInfo item2 in costumeAbilities)
				{
					if (item2.Type == CostumeAbilityType.Booster)
					{
						string valueString = ArgParser.GetValueString(item2.Value, 0);
						int valueInt = ArgParser.GetValueInt(item2.Value, 1);
						dictionary = dictionary ?? new Dictionary<string, int>();
						dictionary[valueString] = (dictionary.ContainsKey(valueString) ? (dictionary[valueString] + valueInt) : valueInt);
					}
				}
			}
		}
		if (dictionary != null)
		{
			CostumeBoosterInfo costumeBoosterInfo = null;
			HashSet<EventBonusType> hashSet = null;
			foreach (KeyValuePair<string, int> item3 in dictionary)
			{
				string key = item3.Key;
				BoosterDefineData data2 = GameRoot.Instance.BoosterDefineDataContainer.GetData(key);
				if (data2 == null)
				{
					VespaDebug.LogError("BoosterDefineData not found : " + key);
					continue;
				}
				int value = item3.Value;
				string stringDirect = LocalizationManager.GetStringDirect(data2.TooltipDesc, value);
				if (costumeBoosterInfo == null)
				{
					costumeBoosterInfo = new CostumeBoosterInfo();
					costumeBoosterInfo.ItemCode = data2.IconItemCode;
					costumeBoosterInfo.BonusType = data2.BonusType;
					costumeBoosterInfo.Value = 0;
					costumeBoosterInfo.Name = LocalizationManager.GetStringDirect(data2.TooltipName);
					costumeBoosterInfo.Desc = stringDirect;
				}
				else
				{
					costumeBoosterInfo.Desc += "\n";
					costumeBoosterInfo.Desc += stringDirect;
					hashSet = hashSet ?? new HashSet<EventBonusType>();
					hashSet.Add(data2.BonusType);
				}
			}
			if (costumeBoosterInfo != null)
			{
				costumeBoosterInfo.ExtraBonusTypes = hashSet?.ToArray();
				list.Add(costumeBoosterInfo);
			}
		}
		return list;
	}

	public List<BoosterInfo> GetLegendCostumeBoosterInfoList(BattleType battleType = BattleType.None)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		Dictionary<string, int> dictionary = null;
		if (GlobalAutoBehaviour<AccountManager>.instance.CostumeManager != null)
		{
			foreach (CostumeInfo item in GlobalAutoBehaviour<AccountManager>.instance.CostumeManager)
			{
				int costumeIndex = item.CostumeIndex;
				CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
				if (data == null || data.CostumeType != EHeroCostumeType.Legend)
				{
					continue;
				}
				List<CostumeAbilityInfo> costumeAbilities = data.GetCostumeAbilities();
				if (__Extension_Method_ICollection__.COUNT(costumeAbilities) == 0)
				{
					continue;
				}
				foreach (CostumeAbilityInfo item2 in costumeAbilities)
				{
					if (item2.Type == CostumeAbilityType.Booster)
					{
						string valueString = ArgParser.GetValueString(item2.Value, 0);
						int valueInt = ArgParser.GetValueInt(item2.Value, 1);
						dictionary = dictionary ?? new Dictionary<string, int>();
						dictionary[valueString] = (dictionary.ContainsKey(valueString) ? (dictionary[valueString] + valueInt) : valueInt);
					}
				}
			}
		}
		if (dictionary != null)
		{
			CostumeBoosterInfo costumeBoosterInfo = null;
			HashSet<EventBonusType> hashSet = null;
			foreach (KeyValuePair<string, int> item3 in dictionary)
			{
				string key = item3.Key;
				BoosterDefineData data2 = GameRoot.Instance.BoosterDefineDataContainer.GetData(key);
				if (data2 == null)
				{
					VespaDebug.LogError("BoosterDefineData not found : " + key);
					continue;
				}
				int value = item3.Value;
				string stringDirect = LocalizationManager.GetStringDirect(data2.TooltipDesc, value);
				if (costumeBoosterInfo == null)
				{
					costumeBoosterInfo = new CostumeBoosterInfo();
					costumeBoosterInfo.ItemCode = data2.IconItemCode;
					costumeBoosterInfo.BonusType = data2.BonusType;
					costumeBoosterInfo.Value = 0;
					costumeBoosterInfo.Name = LocalizationManager.GetStringDirect(data2.TooltipName);
					costumeBoosterInfo.Desc = LocalizationManager.GetStringDirect("COSTUME_LEGEND_BONUS_APPLY");
					costumeBoosterInfo.MoveTaskType = ClientActivityType.OpenLegendCostumeTotalBuffPopup;
				}
				else
				{
					hashSet = hashSet ?? new HashSet<EventBonusType>();
					hashSet.Add(data2.BonusType);
				}
				if (!cachedLegendCostumeEventBonusDic.ContainsKey(data2.BonusType))
				{
					CostumeBoosterInfo costumeBoosterInfo2 = new CostumeBoosterInfo();
					cachedLegendCostumeEventBonusDic.Add(data2.BonusType, costumeBoosterInfo2);
					costumeBoosterInfo2.ItemCode = data2.IconItemCode;
					costumeBoosterInfo2.BonusType = data2.BonusType;
					costumeBoosterInfo2.Value = item3.Value;
					costumeBoosterInfo2.Name = LocalizationManager.GetStringDirect(data2.TooltipName);
					costumeBoosterInfo2.Desc = stringDirect;
				}
				else
				{
					cachedLegendCostumeEventBonusDic[data2.BonusType].Value += item3.Value;
				}
			}
			if (costumeBoosterInfo != null)
			{
				costumeBoosterInfo.ExtraBonusTypes = hashSet?.ToArray();
				list.Add(costumeBoosterInfo);
			}
		}
		return list;
	}

	public int GetLegendCostumeBoosterValue(EventBonusType eventType)
	{
		if (cachedLegendCostumeEventBonusDic == null)
		{
			return 0;
		}
		cachedLegendCostumeEventBonusDic.TryGetValue(eventType, out var value);
		return value?.Value ?? 0;
	}

	public BoosterInfo GetLegendCostumeBooster(EventBonusType eventType)
	{
		if (cachedLegendCostumeEventBonusDic == null)
		{
			return null;
		}
		cachedLegendCostumeEventBonusDic.TryGetValue(eventType, out var value);
		if (value == null)
		{
			return null;
		}
		return value;
	}

	public List<BoosterInfo> GetAllEffectiveLegendCostumeBoosterByType(ECostumeBuffType costumeBuffType)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(cachedLegendCostumeEventBonusDic))
		{
			return list;
		}
		foreach (KeyValuePair<EventBonusType, BoosterInfo> item in cachedLegendCostumeEventBonusDic)
		{
			if (GameRoot.Instance.BoosterDefineDataContainer.GetData(item.Key.ToString()).CostumeBuffType == costumeBuffType)
			{
				list.Add(item.Value);
			}
		}
		return list;
	}

	public int GetEffectiveLegendCostumeBoosterCount()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(cachedLegendCostumeEventBonusDic))
		{
			return 0;
		}
		return __Extension_Method_ICollection__.COUNT(cachedLegendCostumeEventBonusDic);
	}

	public List<BoosterInfo> GetAllEffectiveNpcBuff(BattleType battleType = BattleType.None)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		string stringDirect = LocalizationManager.GetStringDirect("ACCOUNT_BUFF_BOOSTER");
		string itemCode = "BOOSTER_ACCOUNT_OPTION";
		if (battleType == BattleType.None)
		{
			foreach (KeyValuePair<StaminaType, int> item in GlobalAutoBehaviour<AccountManager>.instance.GetActiveStaminaBuffDictionary(filterBoosterIcon: true))
			{
				if (list.Count <= 0)
				{
					NpcBoosterInfo npcBoosterInfo = new NpcBoosterInfo();
					npcBoosterInfo.ItemCode = itemCode;
					npcBoosterInfo.BonusType = EventBonusType.NpcBuff;
					npcBoosterInfo.Value = 0;
					npcBoosterInfo.Name = stringDirect;
					list.Add(npcBoosterInfo);
				}
				string stringDirect2 = LocalizationManager.GetStringDirect(GameRoot.Instance.StaminaDataContainer.GetData(item.Key).HeroBonusTooltipDesc, item.Value);
				if (String_Extension_Method.IsNullOrEmpty(list[0].Desc))
				{
					list[0].Desc = stringDirect2;
					continue;
				}
				list[0].Desc += "\n";
				list[0].Desc += stringDirect2;
			}
		}
		foreach (KeyValuePair<int, int> item2 in GlobalAutoBehaviour<AccountManager>.instance.GetActiveAccountBuffDictionary(filterBoosterIcon: true))
		{
			if (battleType == BattleType.None || GameRoot.Instance.AccountBuffDataContainer.GetData(item2.Key).BattleType.Contains(battleType))
			{
				if (list.Count <= 0)
				{
					NpcBoosterInfo npcBoosterInfo2 = new NpcBoosterInfo();
					npcBoosterInfo2.ItemCode = itemCode;
					npcBoosterInfo2.BonusType = EventBonusType.NpcBuff;
					npcBoosterInfo2.Value = 0;
					npcBoosterInfo2.Name = stringDirect;
					list.Add(npcBoosterInfo2);
				}
				string stringDirect3 = LocalizationManager.GetStringDirect(GameRoot.Instance.AccountBuffDataContainer.GetData(item2.Key).ToolTipDesc, item2.Value);
				if (String_Extension_Method.IsNullOrEmpty(list[0].Desc))
				{
					list[0].Desc = stringDirect3;
					continue;
				}
				list[0].Desc += "\n";
				list[0].Desc += stringDirect3;
			}
		}
		HeroBonusData activeAddRewardBuff = GlobalAutoBehaviour<AccountManager>.instance.GetActiveAddRewardBuff();
		if (activeAddRewardBuff != null)
		{
			BattleType battleType2 = EnumTools.Parse(activeAddRewardBuff.BonusValue1[0], BattleType.None);
			if (battleType == battleType2 && activeAddRewardBuff.BonusValue1.Count() > 2)
			{
				bool result = true;
				bool.TryParse(activeAddRewardBuff.BonusValue1[2], out result);
				BattleContext battleContext = SingletonAuto<BattleContext>.instance;
				if (battleContext != null)
				{
					IDataProvider dataProvider = battleContext.DataProvider;
					if (dataProvider != null)
					{
						CampaignDungeonData linkedDungeonData = dataProvider.GetEnterDungeon().LinkedDungeonData;
						if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(linkedDungeonData.ChapterIndex, linkedDungeonData.DungeonIndex) || result)
						{
							NpcBoosterInfo npcBoosterInfo3 = new NpcBoosterInfo();
							npcBoosterInfo3.ItemCode = activeAddRewardBuff.Icon;
							npcBoosterInfo3.BonusType = EventBonusType.AddReward;
							npcBoosterInfo3.Value = 0;
							npcBoosterInfo3.Name = LocalizationManager.GetStringDirect(activeAddRewardBuff.BonusNameKey);
							npcBoosterInfo3.Desc = LocalizationManager.GetStringDirect(activeAddRewardBuff.BonusDescKey);
							list.Add(npcBoosterInfo3);
						}
					}
				}
			}
		}
		return list;
	}

	public List<BoosterInfo> GetClassBuffBoosterInfoList()
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		list.Clear();
		ClassBuffWrapper classBuffWrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
		if (classBuffWrapper == null)
		{
			return list;
		}
		List<ClassBuffData> classBuffDataListByClassBuffInfo = classBuffWrapper.GetClassBuffDataListByClassBuffInfo();
		if (classBuffDataListByClassBuffInfo == null || classBuffDataListByClassBuffInfo.Count <= 0)
		{
			return list;
		}
		List<CreatureTagType> list2 = new List<CreatureTagType>();
		list2.Clear();
		BoosterInfo boosterInfo = CreateClassBuffBoosterInfo();
		if (boosterInfo == null)
		{
			return list;
		}
		ClassBuffData classBuffData = null;
		List<CreatureTagType> sortCreatureTypeList = FilterHelper.GetListOrderCreatureTagType();
		int index = 0;
		while (index < sortCreatureTypeList.Count)
		{
			classBuffData = classBuffDataListByClassBuffInfo.Find((ClassBuffData data) => data.TagType == sortCreatureTypeList[index]);
			if (classBuffData != null && !list2.Contains(classBuffData.TagType))
			{
				list2.Add(classBuffData.TagType);
				boosterInfo.Desc += classBuffWrapper.LocalizeTotalDesc(classBuffData.TagType);
				if (index < sortCreatureTypeList.Count - 1)
				{
					boosterInfo.Desc += "\n";
				}
			}
			int num = index + 1;
			index = num;
		}
		list.Add(boosterInfo);
		return list;
	}

	private BoosterInfo CreateClassBuffBoosterInfo()
	{
		ClassBuffWrapper classBuffWrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
		if (classBuffWrapper == null)
		{
			return null;
		}
		return new ClassBuffBoosterInfo
		{
			ItemCode = "BOOSTER_CLASS_BUFF",
			BonusType = EventBonusType.ClassBuff,
			Name = classBuffWrapper.LocalizeContentsClassBuff,
			Value = 0,
			MoveTaskType = ClientActivityType.OpenClassBuff
		};
	}

	private List<BoosterInfo> GetPetBuffBoosterInfo(BattleType battleType = BattleType.None)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.PetManager == null || GlobalAutoBehaviour<AccountManager>.instance.PetManager.Count() == 0 || !GlobalAutoBehaviour<PetManager>.isValidInstance)
		{
			return list;
		}
		PetBuffBoosterInfo petBuffBoosterInfo = new PetBuffBoosterInfo
		{
			BonusType = EventBonusType.PetBuff,
			ItemCode = "BOOSTER_PET_BUFF",
			Value = 0,
			MoveTaskType = ClientActivityType.OpenPetTotalBuffPopup,
			BuffPosition = DropBonusEvnetBuffPosition.Top
		};
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (battleType == BattleType.None)
		{
			empty = LocalizationManager.GetStringDirect("PET_BUFF_SKILL_TITLE_MAINUI");
			empty2 = LocalizationManager.GetStringDirect("PET_BUFF_SKILL_DESC_APPLY");
		}
		else
		{
			empty = LocalizationManager.GetStringDirect("PET_BUFF_SKILL_TITLE");
			empty2 = GlobalAutoBehaviour<PetManager>.instance.GetPetBattleOptionBuffDesc();
		}
		petBuffBoosterInfo.Name = empty;
		petBuffBoosterInfo.Desc = empty2;
		list.Add(petBuffBoosterInfo);
		return list;
	}

	public static string GetEventIconName(DropBonusEvent bonusEvent)
	{
		if (bonusEvent == null)
		{
			return string.Empty;
		}
		if (!String_Extension_Method.IsNullOrEmpty(bonusEvent.ItemCode))
		{
			return bonusEvent.ItemCode;
		}
		return "EventBonusType_" + bonusEvent.BonusType;
	}

	private List<BoosterInfo> GetAllEffectHotTimeEvent(BattleType battleType = BattleType.None)
	{
		List<BoosterInfo> list = new List<BoosterInfo>();
		foreach (DropBonusEvent item in GlobalAutoBehaviour<DropBonusEventManager>.instance.GetAllEffectiveEvent(battleType))
		{
			if (item != null && (item.Title != null || (item.LocalInfo != null && item.LocalInfo.Length != 0)) && Vespa_String_Extension_Method2.TryParseNetworkTime2(item.EndTime, out var o_dateTime))
			{
				HotTimeEventBoosterInfo hotTimeEventBoosterInfo = new HotTimeEventBoosterInfo();
				hotTimeEventBoosterInfo.BattleTypes = item.DungeonType;
				hotTimeEventBoosterInfo.ItemCode = GetEventIconName(item);
				hotTimeEventBoosterInfo.BonusType = item.BonusType;
				hotTimeEventBoosterInfo.Value = item.BonusPercentage;
				hotTimeEventBoosterInfo.EndTimeUTC = o_dateTime;
				hotTimeEventBoosterInfo.MoveTaskType = item.MoveTaskType;
				hotTimeEventBoosterInfo.MoveTaskValue = new string[1] { "" };
				if (item.MoveTaskValue != null)
				{
					hotTimeEventBoosterInfo.MoveTaskValue = Vespa_String_Extension_Method.TRIM(item.MoveTaskValue).Split(',');
				}
				hotTimeEventBoosterInfo.MinLevel = item.MinLevel;
				DropBonusEventLocalInfo localInfoOfCurLanguage = DropBonusEventManager.GetLocalInfoOfCurLanguage(item);
				EventBonusAttribute eventBonusAttribute = EnumAttributeContainer<EventBonusType, EventBonusAttribute>.Get(item.BonusType);
				string text = ((eventBonusAttribute != null) ? eventBonusAttribute.PercentPrefix : item.BonusType.ToString());
				hotTimeEventBoosterInfo.Name = ((localInfoOfCurLanguage != null && localInfoOfCurLanguage.Title != null) ? localInfoOfCurLanguage.Title : text);
				hotTimeEventBoosterInfo.Desc = ((localInfoOfCurLanguage != null && localInfoOfCurLanguage.Message != null) ? localInfoOfCurLanguage.Message : (text + " +" + item.BonusPercentage + "%"));
				hotTimeEventBoosterInfo.BeginHour = item.BeginHour;
				hotTimeEventBoosterInfo.BeginMinute = item.BeginMinute;
				hotTimeEventBoosterInfo.EndHour = item.EndHour;
				hotTimeEventBoosterInfo.EndMinute = item.EndMinute;
				hotTimeEventBoosterInfo.BuffPosition = item.BuffPosition;
				list.Add(hotTimeEventBoosterInfo);
			}
		}
		return list;
	}

	public void InitAllEffectiveBooster(BattleType battleType, bool includeCash)
	{
		_battleType = battleType;
		_includeCash = includeCash;
		InitAllEffectiveBooster();
	}

	private void InitAllEffectiveBooster()
	{
		BattleType battleType = _battleType;
		oldBoosterInfoList.Clear();
		oldBoosterInfoList.AddRange(BoosterInfoList);
		cachedLegendCostumeEventBonusDic.Clear();
		BoosterInfoList.Clear();
		BoosterInfoList.AddRange(GetAllEffectiveBoosterItem(battleType));
		BoosterInfoList.AddRange(GetAllEffectiveFixedTerm(battleType));
		BoosterInfoList.AddRange(GetAllEffectHotTimeEvent(battleType));
		BoosterInfoList.AddRange(GetNormalCostumeBoosterInfoList(battleType));
		BoosterInfoList.AddRange(GetLegendCostumeBoosterInfoList(battleType));
		BoosterInfoList.AddRange(GetAllEffectiveGuildSkill(battleType));
		BoosterInfoList.AddRange(GetAllEffectiveNpcBuff(battleType));
		BoosterInfoList.AddRange(GetClassBuffBoosterInfoList());
		BoosterInfoList.AddRange(GetPetBuffBoosterInfo(battleType));
		if (_includeCash)
		{
			BoosterInfoList.AddRange(GetAllEffectiveCash(battleType));
		}
		BoosterInfoList.Sort((BoosterInfo x, BoosterInfo y) => x.GetSortScore().CompareTo(y.GetSortScore()));
	}

	public void AnnounceChangedBoosterInfoList(BoosterInfo expiredBooster = null)
	{
		bool flag = false;
		if (__Extension_Method_ICollection__.COUNT(oldBoosterInfoList) != __Extension_Method_ICollection__.COUNT(BoosterInfoList))
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(oldBoosterInfoList); i++)
			{
				BoosterInfo boosterInfo = oldBoosterInfoList[i];
				BoosterInfo other = BoosterInfoList[i];
				if (!boosterInfo.CheckEqualForModification(other))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (WeakRef<IBoosterListObserver> observer in _observerList)
		{
			if (observer.Value != null)
			{
				observer.Value.OnChanged(expiredBooster);
			}
		}
	}

	private void RefreshAllEffectiveBooster(BoosterInfo expiredBooster = null)
	{
		InitAllEffectiveBooster();
		AnnounceChangedBoosterInfoList(expiredBooster);
	}

	public List<BoosterInfo> GetAllEffectiveBooster()
	{
		return BoosterInfoList;
	}

	public string GetBoosterCode(EventBonusType bonusType)
	{
		foreach (BoosterInfo boosterInfo in BoosterInfoList)
		{
			if (boosterInfo.BonusType == bonusType)
			{
				return boosterInfo.ItemCode;
			}
			if (boosterInfo.ExtraBonusTypes != null && boosterInfo.ExtraBonusTypes.Contains(bonusType))
			{
				return boosterInfo.ItemCode;
			}
		}
		return string.Empty;
	}

	public static string GetBoosterRemainTime(int remainTime)
	{
		string text = "";
		if (new TimeSpan(0, 0, remainTime).TotalDays >= 1.0)
		{
			int showSec = 0;
			int showMin = 1;
			int showHour = 1;
			int showDay = 1;
			return UIHelper.GetRemainTimeTextAll(remainTime, (byte)showDay != 0, (byte)showHour != 0, (byte)showMin != 0, (byte)showSec != 0);
		}
		int showSec2 = 1;
		int showMin2 = 1;
		int showHour2 = 1;
		int showDay2 = 1;
		return UIHelper.GetRemainTimeTextAll(remainTime, (byte)showDay2 != 0, (byte)showHour2 != 0, (byte)showMin2 != 0, (byte)showSec2 != 0);
	}

	public List<int> GetNewBoosterItemIndexList()
	{
		RemoveExpiredNewBoosterItemIndex();
		return newBoosterItemIndexList;
	}

	public void ClearNewBoosterItemIndexList()
	{
		newBoosterItemIndexList.Clear();
	}

	private void AddNewBoosterItemIndex(int itemIndex)
	{
		if (!newBoosterItemIndexList.Contains(itemIndex))
		{
			newBoosterItemIndexList.Add(itemIndex);
		}
	}

	private void RemoveExpiredNewBoosterItemIndex()
	{
		List<int> expiredList = new List<int>();
		foreach (int index in newBoosterItemIndexList)
		{
			if (BoosterInfoList.Find((BoosterInfo item) => (item.Type == BoosterInfoType.BoosterItem && ((ItemBoosterInfo)item).ItemIndex == index) ? true : false) == null)
			{
				expiredList.Add(index);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(expiredList) > 0)
		{
			newBoosterItemIndexList.RemoveAll((int item) => expiredList.Contains(item));
		}
	}

	public SaveStaminaBoosterInfo CheckSaveStamina()
	{
		SaveStaminaBoosterInfo saveStaminaBoosterInfo = new SaveStaminaBoosterInfo();
		foreach (BoosterInfo boosterInfo in BoosterInfoList)
		{
			if (boosterInfo.BonusType == EventBonusType.SaveStamina && boosterInfo.Value > 0)
			{
				saveStaminaBoosterInfo.SaveStaminaValue = boosterInfo.Value + saveStaminaBoosterInfo.SaveStaminaValue;
				saveStaminaBoosterInfo.BattleTypeList.AddRange(boosterInfo.BattleTypes);
			}
		}
		saveStaminaBoosterInfo.BattleTypeList = saveStaminaBoosterInfo.BattleTypeList.Distinct().ToList();
		return saveStaminaBoosterInfo;
	}

	void IUserObserver<ClassBuffInfo>.OnChanged(ClassBuffInfo changedInfo)
	{
		RefreshAllEffectiveBooster();
	}
}
