using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NAccount;

public class UserManager
{
	public class UserStatTypeAttribute : Attribute
	{
		public CurrencyType CurrencyType;

		private static bool _initConvert;

		private static Dictionary<CurrencyType, UserStatType> _currencyToUserStatType;

		private static Dictionary<UserStatType, CurrencyType> _userStatTypeToCurrency;

		private static void Init()
		{
			if (_initConvert)
			{
				return;
			}
			_initConvert = true;
			_currencyToUserStatType = new Dictionary<CurrencyType, UserStatType>();
			_userStatTypeToCurrency = new Dictionary<UserStatType, CurrencyType>();
			EnumAttributeContainer<UserStatType, UserStatTypeAttribute>.ForEach(delegate(UserStatType type, UserStatTypeAttribute attr)
			{
				if (attr.CurrencyType != CurrencyType.None)
				{
					if (_currencyToUserStatType.ContainsKey(attr.CurrencyType))
					{
						Debug.LogError("이미 포함된 CurrencyType이다: " + attr.CurrencyType);
					}
					else
					{
						_currencyToUserStatType.Add(attr.CurrencyType, type);
					}
					if (_userStatTypeToCurrency.ContainsKey(type))
					{
						Debug.LogError("이미 포함된 UserStatType 이다: " + type);
					}
					else
					{
						_userStatTypeToCurrency.Add(type, attr.CurrencyType);
					}
				}
			});
		}

		public static UserStatType ToUserStatType(CurrencyType currencyType)
		{
			Init();
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_currencyToUserStatType, currencyType, UserStatType.None);
		}

		public static bool TryToCurrency(UserStatType userStatType, out CurrencyType currencyType)
		{
			Init();
			return _userStatTypeToCurrency.TryGetValue(userStatType, out currencyType);
		}
	}

	public enum UserStatType
	{
		None,
		[UserStatType(CurrencyType = CurrencyType.Gold)]
		Gold,
		[UserStatType(CurrencyType = CurrencyType.Gem)]
		Gem,
		PayGem,
		[UserStatType(CurrencyType = CurrencyType.PvpCoin)]
		PvpCoin,
		Stamina,
		StaminaRechargeTime,
		StaminaRechargeCount,
		TeamLevel,
		TeamExp,
		PlunderSearch,
		QuickCompleteCollect,
		MatchScore,
		SeasonWin,
		SeasonLose,
		SuccessiveWin,
		SuccessiveLose,
		TierIndex,
		InventoryExtend,
		AvatarHeroIndex,
		PlayerAvatarHeroInfo,
		TrialDungeonKey,
		RoyalPoint,
		[UserStatType(CurrencyType = CurrencyType.Mileage)]
		Mileage,
		ScenarioQuestIndex,
		ScenarioQuestProgress,
		[UserStatType(CurrencyType = CurrencyType.FriendshipPoint)]
		FriendshipPoint,
		BonusRaidOccurPercent,
		LastRaidResetTime,
		[UserStatType(CurrencyType = CurrencyType.GuildPoint)]
		GuildPoint,
		DailyAccGuildPoint,
		DailyAccFriendshipPoint,
		[UserStatType(CurrencyType = CurrencyType.RaidPoint)]
		RaidPoint,
		[UserStatType(CurrencyType = CurrencyType.WorldBossPoint)]
		WorldBossPoint,
		Nick,
		[UserStatType(CurrencyType = CurrencyType.EventDungeonPoint)]
		EventDungeonPoint,
		[UserStatType(CurrencyType = CurrencyType.EventDungeonPoint2)]
		EventDungeonPoint2,
		[UserStatType(CurrencyType = CurrencyType.GloryPoint)]
		GloryPoint,
		[UserStatType(CurrencyType = CurrencyType.EventGiftPoint)]
		EventGiftPoint,
		EventDungeonDailyPoint,
		[UserStatType(CurrencyType = CurrencyType.LuaPoint)]
		LuaPoint,
		[UserStatType(CurrencyType = CurrencyType.MazeKey)]
		MazeKey,
		[UserStatType(CurrencyType = CurrencyType.RankingPoint)]
		RankingPoint,
		[UserStatType(CurrencyType = CurrencyType.EventDungeonPoint3)]
		EventDungeonPoint3,
		SubStoryAvatarHeroIndex,
		[UserStatType(CurrencyType = CurrencyType.GuildArenaKey)]
		GuildArenaKey,
		[UserStatType(CurrencyType = CurrencyType.GuildArenaPoint)]
		GuildArenaPoint,
		MainQuest,
		[UserStatType(CurrencyType = CurrencyType.EclipseKey)]
		EclipseKey,
		[UserStatType(CurrencyType = CurrencyType.EclipsePoint)]
		EclipsePoint,
		SoulWeaponMilige,
		[UserStatType(CurrencyType = CurrencyType.ShopEventPoint)]
		ShopEventPoint,
		[UserStatType(CurrencyType = CurrencyType.ShakmehMiddleBossKey)]
		ShakmehMiddleBossKey,
		[UserStatType(CurrencyType = CurrencyType.ShakmehMiddleBossPoint)]
		ShakmehMiddleBossPoint,
		[UserStatType(CurrencyType = CurrencyType.LimitedShopEventPoint)]
		LimitedShopEventPoint,
		[UserStatType(CurrencyType = CurrencyType.OrdealArenaPoint)]
		OrdealArenaPoint,
		[UserStatType(CurrencyType = CurrencyType.TreasureHousePoint)]
		TreasureHousePoint,
		[UserStatType(CurrencyType = CurrencyType.EventOrvelPoint)]
		EventOrvelPoint,
		[UserStatType(CurrencyType = CurrencyType.ChallengeRaidPoint)]
		ChallengeRaidPoint,
		[UserStatType(CurrencyType = CurrencyType.PunishmentRaidKey)]
		PunishmentRaidKey,
		[UserStatType(CurrencyType = CurrencyType.CraftEventPoint)]
		CraftEventPoint,
		[UserStatType(CurrencyType = CurrencyType.GrowWorldTreePoint)]
		GrowWorldTreePoint
	}

	public class UserStatInfo
	{
		public UserStatType UserStatType { get; set; }

		public StaminaType StaminaType { get; set; }

		public long BeforeValue { get; set; }

		public long AfterValue { get; set; }

		public long BeforeValue2 { get; set; }

		public long AfterValue2 { get; set; }

		public string BeforeValue3 { get; set; }

		public string AfterValue3 { get; set; }
	}

	public class CustomMiscInfo
	{
		public GuildSuppressSessionInfo GuildSuppressSessionInfo { get; set; }

		public PlayerTreasureHouseInfo PlayerTreasureHouseInfo { get; set; }
	}

	private readonly AccountManager _accountManager;

	private readonly UserInfo _userInfo;

	private PlayerAvatarHeroInfo _playerAvatarHeroInfo;

	private PlayerMiscInfo _miscInfo;

	private PlayerGuildPointInfo _guildPointInfo;

	private readonly UserObserverManager<UserStatInfo> _observerManager = new UserObserverManager<UserStatInfo>();

	private UserStaminaManager _staminaManager;

	private CustomMiscInfo _customMiscInfo;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserInfo UserInfo => _userInfo;

	public long Id => _userInfo.AccountId;

	public string Nick => _userInfo.Nick;

	public long Gold => _userInfo.Gold;

	public long Gem => _userInfo.Gem;

	public long PayGem => _userInfo.PayGem;

	public long TotalGem => checked(_userInfo.Gem + _userInfo.PayGem);

	public long Stamina => _userInfo.Stamina;

	public string StaminaRechargeTime => _userInfo.StaminaRechargeTime;

	public long Sword => _userInfo.Sword;

	public string SwordRechargeTime => _userInfo.SwordRechargeTime;

	public int TeamLevel => _userInfo.TeamLevel;

	public int TeamExp => _userInfo.TeamExp;

	public int AvatarHeroIndex => _userInfo.AvatarHeroIndex;

	public long GuildPoint => _guildPointInfo.GuildPoint;

	public long RoyalPoint => _userInfo.RoyalPoint;

	public long PvpCoin => _userInfo.PvpCoin;

	public long UndergroundPrisonKey => _userInfo.UndergroundPrisonKey;

	public string UndergroundPrisonKeyRechargeTime => _userInfo.UndergroundPrisonKeyRechargeTime;

	public long UndergroundLabyrinthKey => _userInfo.UndergroundLabyrinthKey;

	public string UndergroundLabyrinthKeyRechargeTime => _userInfo.UndergroundLabyrinthKeyRechargeTime;

	public long Mileage => _userInfo.Mileage;

	public long FriendshipPoint => _userInfo.FriendshipPoint;

	public long RaidPoint => _userInfo.RaidPoint;

	public long WorldBossPoint => _userInfo.WorldBossPoint;

	public long EventDungeonTicket => _userInfo.EventDungeonTicket;

	public long EventHeroDungeonTicket => _userInfo.EventHeroDungeonTicket;

	public long EventDungeonPoint => _userInfo.EventDungeonPoint;

	public long EventDungeonDailyPoint => _userInfo.EventDungeonDailyPoint;

	public long EventDungeonPoint2 => _userInfo.EventDungeonPoint2;

	public long GloryPoint => _userInfo.GloryPoint;

	public long LuaPoint => _userInfo.LuaPoint;

	public long Sword2 => _userInfo.Sword2;

	public string Sword2RechargeTime => _userInfo.Sword2RechargeTime;

	public long MazeKey => _userInfo.MazeKey;

	public UserStaminaManager StaminaManager => _staminaManager;

	public PlayerAvatarHeroInfo PlayerAvatarHeroInfo => _playerAvatarHeroInfo;

	public PlayerMiscInfo MiscInfo => _miscInfo;

	public PlayerGuildPointInfo GuildPointInfo => _guildPointInfo;

	public PlayerTreasureHouseInfo TreasureHouseInfo => _customMiscInfo.PlayerTreasureHouseInfo;

	public int InventoryExtend => _miscInfo.InventoryExtend;

	public int ChestExtend => _miscInfo.ChestExtend;

	public PushRewardInfo DailyGiftBoxInfo { get; set; }

	public bool IsSelectingSoulStone
	{
		get
		{
			if (_miscInfo == null)
			{
				return false;
			}
			return !String_Extension_Method.IsNullOrEmpty(_miscInfo.RestoreSoulStoneIndices);
		}
	}

	public int[] RestoreSoulStoneIndices
	{
		get
		{
			if (!IsSelectingSoulStone)
			{
				return null;
			}
			List<int> list = new List<int>();
			string[] array = _miscInfo.RestoreSoulStoneIndices.Replace("[", "").Replace("]", "").Trim()
				.Split(',');
			foreach (string value in array)
			{
				list.Add(Convert.ToInt32(value));
			}
			return list.ToArray();
		}
	}

	public long SoulStoneMileage
	{
		get
		{
			if (_miscInfo == null)
			{
				int num = 0;
				return num;
			}
			return _miscInfo.SoulStoneMileage;
		}
	}

	public UserManager(AccountManager accountManager, UserInfo userInfo, PlayerMiscInfo miscInfo, PlayerGuildPointInfo guildPointInfo)
	{
		_accountManager = accountManager;
		_userInfo = userInfo;
		_miscInfo = miscInfo;
		_guildPointInfo = guildPointInfo;
		_staminaManager = new UserStaminaManager(_accountManager, this);
		_customMiscInfo = new CustomMiscInfo();
	}

	public void Update()
	{
		_staminaManager.Update();
	}

	public long GetStamina(StaminaType staminaType)
	{
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(staminaType);
		if (data == null)
		{
			int num = 0;
			return num;
		}
		return GetStamina(data);
	}

	public long GetStamina(StaminaData staminaData)
	{
		if (staminaData == null)
		{
			int num = 0;
			return num;
		}
		string attributeName = staminaData.AttributeName;
		int num2 = 0;
		long val = num2;
		if (!CustomJsonMarshaler.CustomUserInfoParseField<long>(_userInfo, attributeName, out val))
		{
			int num3 = 0;
			return num3;
		}
		return val;
	}

	public int GetStaminaRechargeCount(StaminaData staminaData)
	{
		if (staminaData == null)
		{
			return 0;
		}
		string name = staminaData.AttributeName + "RechargeCount";
		int val = 0;
		if (!CustomJsonMarshaler.CustomUserInfoParseField<int>(_userInfo, name, out val))
		{
			return 0;
		}
		return val;
	}

	public void SetStamina(StaminaType type, long after)
	{
		if (GameRoot.Instance == null || GameRoot.Instance.StaminaDataContainer == null)
		{
			return;
		}
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(type);
		if (data == null)
		{
			return;
		}
		string attributeName = data.AttributeName;
		long stamina = GetStamina(data);
		if (stamina != after)
		{
			if (!CustomJsonMarshaler.CustomUserInfoSetField(_userInfo, attributeName, after))
			{
				VespaDebug.LogError("Failed to set UserInfo." + attributeName + " to " + after + " (before=" + stamina + ")");
			}
			else
			{
				_observerManager.AnnounceToObservers(new UserStatInfo
				{
					UserStatType = UserStatType.Stamina,
					StaminaType = type,
					BeforeValue = stamina,
					AfterValue = after
				});
			}
		}
	}

	public void SetStaminaRechargeTime(StaminaType type, string after)
	{
		SetStaminaSubInfo(type, after, "RechargeTime", UserStatType.StaminaRechargeTime);
	}

	public void SetStaminaRechargeCount(StaminaType type, int after)
	{
		SetStaminaSubInfo(type, after, "RechargeCount", UserStatType.StaminaRechargeCount);
	}

	private void SetStaminaSubInfo<T>(StaminaType type, T after, string attrPostFix, UserStatType userStatType)
	{
		if (GameRoot.Instance == null || GameRoot.Instance.StaminaDataContainer == null)
		{
			return;
		}
		StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(type);
		if (data == null)
		{
			return;
		}
		string text = data.AttributeName + attrPostFix;
		T val = default(T);
		if (CustomJsonMarshaler.CustomUserInfoParseField<T>(_userInfo, text, out val) && !(BaseJsonMarshaler.CastToString(val) == BaseJsonMarshaler.CastToString(after)))
		{
			if (!CustomJsonMarshaler.CustomUserInfoSetField(_userInfo, text, after))
			{
				Action<object> logError = VespaDebug.LogError;
				string[] obj = new string[7] { "Failed to set UserInfo.", text, " to ", null, null, null, null };
				T val2 = after;
				obj[3] = val2?.ToString();
				obj[4] = " (before=";
				val2 = val;
				obj[5] = val2?.ToString();
				obj[6] = ")";
				logError(string.Concat(obj));
			}
			else
			{
				_observerManager.AnnounceToObservers(new UserStatInfo
				{
					UserStatType = userStatType,
					StaminaType = type
				});
			}
		}
	}

	public void SetNick(string nick)
	{
		string nick2 = _userInfo.Nick;
		_userInfo.Nick = nick;
		_observerManager.AnnounceToObservers(new UserStatInfo
		{
			UserStatType = UserStatType.Nick,
			BeforeValue3 = nick2,
			AfterValue3 = nick
		});
	}

	public void SetUserStat(UserStatType type, long value)
	{
		long userStat = GetUserStat(type);
		if (userStat != value)
		{
			string text = type.ToString();
			if (CustomJsonMarshaler.CustomUserInfoSetField(_userInfo, text, value))
			{
				_observerManager.AnnounceToObservers(new UserStatInfo
				{
					UserStatType = type,
					BeforeValue = userStat,
					AfterValue = value
				});
			}
			else
			{
				VespaDebug.LogError("Undefined user stat: " + text);
			}
		}
	}

	public void SetUserCurrency(CurrencyType currencyType, long value)
	{
		if (currencyType != CurrencyType.None)
		{
			UserStatType userStatType = UserStatTypeAttribute.ToUserStatType(currencyType);
			if (userStatType == UserStatType.None)
			{
				VespaDebug.LogError("UserStatType of currency not found : " + currencyType);
			}
			else
			{
				SetUserStat(userStatType, value);
			}
		}
	}

	public long GetUserStat(UserStatType type)
	{
		return GetUserStat(type.ToString());
	}

	public long GetUserStat(string statName)
	{
		long val = 0L;
		if (CustomJsonMarshaler.CustomUserInfoParseField<long>(_userInfo, statName, out val))
		{
			return val;
		}
		if (string.Equals(statName, UserStatType.GuildPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GuildPoint;
		}
		if (string.Equals(statName, UserStatType.EventDungeonPoint3.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.EventDungeonPoint3.ToString());
		}
		if (string.Equals(statName, UserStatType.RankingPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.RankingPoint.ToString());
		}
		if (string.Equals(statName, UserStatType.GuildArenaPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.GuildArenaPoint.ToString());
		}
		if (string.Equals(statName, UserStatType.EclipsePoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.EclipsePoint.ToString());
		}
		if (string.Equals(statName, UserStatType.ShopEventPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.ShopEventPoint.ToString());
		}
		if (string.Equals(statName, UserStatType.ShakmehMiddleBossPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.ShakmehMiddleBossPoint.ToString());
		}
		if (string.Equals(statName, UserStatType.LimitedShopEventPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.LimitedShopEventPoint.ToString());
		}
		if (string.Equals(statName, UserStatType.OrdealArenaPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.OrdealArenaPoint.ToString());
		}
		if (string.Equals(statName, UserStatType.TreasureHousePoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.TreasureHousePoint.ToString());
		}
		if (string.Equals(statName, UserStatType.EventOrvelPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.EventOrvelPoint.ToString());
		}
		if (string.Equals(statName, UserStatType.ChallengeRaidPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.ChallengeRaidPoint.ToString());
		}
		if (string.Equals(statName, UserStatType.PunishmentRaidKey.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.PunishmentRaidKey.ToString());
		}
		if (string.Equals(statName, UserStatType.CraftEventPoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.CraftEventPoint.ToString());
		}
		if (string.Equals(statName, UserStatType.GrowWorldTreePoint.ToString(), StringComparison.InvariantCultureIgnoreCase))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(UserStatType.GrowWorldTreePoint.ToString());
		}
		VespaDebug.LogError("Undefined user stat: " + statName);
		return 0L;
	}

	public void SetGold(long value)
	{
		SetUserStat(UserStatType.Gold, value);
	}

	public void SetTeamLevel(int value)
	{
		SetUserStat(UserStatType.TeamLevel, value);
	}

	public void SetGem(long value)
	{
		SetUserStat(UserStatType.Gem, value);
	}

	public void SetPayGem(long value)
	{
		SetUserStat(UserStatType.PayGem, value);
	}

	public void SetRoyalPoint(long value)
	{
		SetUserStat(UserStatType.RoyalPoint, value);
	}

	public void SetMileage(long value)
	{
		SetUserStat(UserStatType.Mileage, value);
	}

	public void SetLuaPoint(long value)
	{
		SetUserStat(UserStatType.LuaPoint, value);
	}

	public void SetFriendshipPoint(long value)
	{
		SetUserStat(UserStatType.FriendshipPoint, value);
	}

	public void SetWorldBossPoint(long value)
	{
		SetUserStat(UserStatType.WorldBossPoint, value);
	}

	public void SetEventDungeonPoint(long value)
	{
		SetUserStat(UserStatType.EventDungeonPoint, value);
	}

	public void SetEventDungeonDailyPoint(long value)
	{
		SetUserStat(UserStatType.EventDungeonDailyPoint, value);
	}

	public void SetEventDungeonPoint2(long value)
	{
		SetUserStat(UserStatType.EventDungeonPoint2, value);
	}

	public void SetEventGiftPoint(long value)
	{
		SetUserStat(UserStatType.EventGiftPoint, value);
	}

	public void SetEventDungeonPoint3(long value)
	{
		SetUserStat(UserStatType.EventDungeonPoint3, value);
	}

	public void SetTeamExp(int team_exp, int team_level)
	{
		int teamExp = _userInfo.TeamExp;
		_userInfo.TeamExp = team_exp;
		if (teamExp != team_exp)
		{
			_observerManager.AnnounceToObservers(new UserStatInfo
			{
				UserStatType = UserStatType.TeamExp,
				BeforeValue = teamExp,
				AfterValue = team_exp,
				BeforeValue2 = _userInfo.TeamLevel,
				AfterValue2 = team_level
			});
		}
	}

	public void SetDailyAccFriendshipPoint(long DailyAccFriendshipPoint)
	{
		long dailyAccFriendshipPoint = MiscInfo.DailyAccFriendshipPoint;
		MiscInfo.DailyAccFriendshipPoint = DailyAccFriendshipPoint;
		if (dailyAccFriendshipPoint != DailyAccFriendshipPoint)
		{
			_observerManager.AnnounceToObservers(new UserStatInfo
			{
				UserStatType = UserStatType.DailyAccFriendshipPoint,
				BeforeValue = dailyAccFriendshipPoint,
				AfterValue = DailyAccFriendshipPoint
			});
		}
	}

	public void SetLastRaidResetTime(string time)
	{
		string lastRaidResetTime = MiscInfo.LastRaidResetTime;
		MiscInfo.LastRaidResetTime = time;
		if (lastRaidResetTime != time)
		{
			_observerManager.AnnounceToObservers(new UserStatInfo
			{
				UserStatType = UserStatType.LastRaidResetTime
			});
		}
	}

	public void ParseCurrencyResult(CurrencyResultInfo info, List<GoldResultInfo> goldResultInfoList, List<GemResultInfo> gemResultInfoList)
	{
		if (info == null || string.IsNullOrEmpty(info.ResultString))
		{
			return;
		}
		try
		{
			switch (info.CurrencyType)
			{
			case RewardType.Gold:
				goldResultInfoList.Add(JsonMarshaler.Parse<GoldResultInfo>(info.ResultString));
				break;
			case RewardType.Gem:
				gemResultInfoList.Add(JsonMarshaler.Parse<GemResultInfo>(info.ResultString));
				break;
			}
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
	}

	public void Apply(IList<GoldResultInfo> goldResults, IList<GemResultInfo> gemResults, IList<CurrencyResultInfo> infos)
	{
		List<GoldResultInfo> list = new List<GoldResultInfo>();
		List<GemResultInfo> list2 = new List<GemResultInfo>();
		if (__Extension_Method_ICollection__.COUNT(goldResults) > 0)
		{
			list.AddRange(goldResults);
		}
		if (__Extension_Method_ICollection__.COUNT(gemResults) > 0)
		{
			list2.AddRange(gemResults);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			CurrencyResultInfo info = infos[i];
			ParseCurrencyResult(info, list, list2);
		}
		Apply(list);
		Apply(list2);
	}

	public void Apply(IList<CurrencyResultInfo2> infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
			{
				CurrencyResultInfo2 currencyResultInfo = infos[i];
				CurrencyType currencyType = currencyResultInfo.CurrencyType;
				long newValue = currencyResultInfo.NewValue;
				SetUserCurrency(currencyType, newValue);
			}
		}
	}

	public void Apply(StaminaResultInfo info)
	{
		if (info != null)
		{
			Apply(info.SprkRaiderExp);
		}
		if (info != null)
		{
			SetStamina(info.Type, info.NewValue);
			SetStaminaRechargeTime(info.Type, info.StaminaRechargeTime);
			SetStaminaRechargeCount(info.Type, info.RechargeCount);
			_staminaManager.ApplyExpectedRechargeTime(info.Type, info.NextRechargeRemainTime, info.FullRechargeRemainTime);
		}
	}

	public void Apply(IList<StaminaResultInfo> infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Count; i++)
			{
				Apply(infos[i]);
			}
		}
	}

	public void Apply(CurrencyResultInfo3 info)
	{
		if (info != null)
		{
			switch (info.CurrencyType)
			{
			case CurrencyType.Gold:
				SetGold(info.NewValue);
				break;
			case CurrencyType.Gem:
				SetGem(info.GetFieldValue("NewSysGem"));
				SetPayGem(info.GetFieldValue("NewPayGem"));
				break;
			case CurrencyType.Mileage:
				SetMileage(info.NewValue);
				break;
			case CurrencyType.LuaPoint:
				SetLuaPoint(info.NewValue);
				break;
			case CurrencyType.WorldBossPoint:
				SetWorldBossPoint(info.NewValue);
				break;
			case CurrencyType.EventDungeonPoint:
				SetEventDungeonPoint(info.NewValue);
				break;
			case CurrencyType.EventDungeonPoint2:
				SetEventDungeonPoint2(info.NewValue);
				break;
			case CurrencyType.EventGiftPoint:
				SetEventGiftPoint(info.NewValue);
				break;
			case CurrencyType.EventDungeonPoint3:
				SetEventDungeonPoint3(info.NewValue);
				break;
			case CurrencyType.PvpCoin:
			case CurrencyType.RaidPoint:
			case CurrencyType.GloryPoint:
			case CurrencyType.EventOrvelPoint:
				SetUserCurrency(info.CurrencyType, info.NewValue);
				break;
			case CurrencyType.RankingPoint:
			case CurrencyType.GuildArenaPoint:
			case CurrencyType.EclipsePoint:
			case CurrencyType.ShopEventPoint:
			case CurrencyType.LimitedShopEventPoint:
			case CurrencyType.OrdealArenaPoint:
			case CurrencyType.TreasureHousePoint:
			case CurrencyType.ChallengeRaidPoint:
			case CurrencyType.CraftEventPoint:
			case CurrencyType.GrowWorldTreePoint:
				GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(info);
				break;
			case CurrencyType.FriendshipPoint:
				SetFriendshipPoint(info.NewValue);
				break;
			case CurrencyType.GuildPoint:
				SetGuildPoint(info.NewValue);
				break;
			case CurrencyType.ShakmehMiddleBossKey:
				SetUserCurrency(info.CurrencyType, info.NewValue);
				break;
			case CurrencyType.ShakmehMiddleBossPoint:
				GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(info);
				_observerManager.AnnounceToObservers(new UserStatInfo
				{
					UserStatType = UserStatType.ShakmehMiddleBossPoint,
					BeforeValue = checked(info.NewValue - info.AddValue),
					AfterValue = info.NewValue
				});
				break;
			}
		}
	}

	public void Apply(IList<CurrencyResultInfo3> infos)
	{
		if (__Extension_Method_ICollection__.COUNT(infos) != 0)
		{
			List<CurrencyResultInfo3> list = new List<CurrencyResultInfo3>();
			list.AddRange(infos);
			list.Sort((CurrencyResultInfo3 l, CurrencyResultInfo3 r) => l.GetFieldValue("Sequence").CompareTo(r.GetFieldValue("Sequence")));
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
			{
				Apply(list[num]);
			}
		}
	}

	public void Apply(GoldResultInfo info)
	{
		if (info != null)
		{
			SetGold(info.NewValue);
		}
	}

	public void Apply(IList<GoldResultInfo> infos)
	{
		if (__Extension_Method_ICollection__.COUNT(infos) != 0)
		{
			List<GoldResultInfo> list = new List<GoldResultInfo>();
			list.AddRange(infos);
			list.Sort(delegate(GoldResultInfo l, GoldResultInfo r)
			{
				long sequence = l.Sequence;
				long sequence2 = r.Sequence;
				return sequence.CompareTo(checked((int)sequence2));
			});
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
			{
				Apply(list[num]);
			}
		}
	}

	public void Apply(GemResultInfo info)
	{
		if (info != null)
		{
			SetGem(info.NewSysGem);
			SetPayGem(info.NewPayGem);
		}
	}

	public void Apply(IList<GemResultInfo> infos)
	{
		if (__Extension_Method_ICollection__.COUNT(infos) != 0)
		{
			List<GemResultInfo> list = new List<GemResultInfo>();
			list.AddRange(infos);
			list.Sort(delegate(GemResultInfo l, GemResultInfo r)
			{
				long sequence = l.Sequence;
				long sequence2 = r.Sequence;
				return sequence.CompareTo(checked((int)sequence2));
			});
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
			{
				Apply(list[num]);
			}
		}
	}

	public void Apply(MileageResultInfo info)
	{
		if (info != null)
		{
			SetMileage(info.NewValue);
		}
	}

	public void Apply(IList<MileageResultInfo> infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Count; i++)
			{
				Apply(infos[i]);
			}
		}
	}

	public void Apply(WorldBossPointResultInfo info)
	{
		if (info != null)
		{
			SetWorldBossPoint(info.NewValue);
		}
	}

	public void Apply(EventDungeonPointResultInfo info)
	{
		if (info != null)
		{
			SetEventDungeonPoint(info.NewValue);
			SetEventDungeonDailyPoint(info.DayValue);
		}
	}

	public void Apply(EventDungeonPoint2ResultInfo info)
	{
		if (info != null)
		{
			SetEventDungeonPoint2(info.NewValue);
		}
	}

	public void Apply(EventGiftPointResultInfo info)
	{
		if (info != null)
		{
			SetEventGiftPoint(info.NewValue);
		}
	}

	public void Apply(IList<WorldBossPointResultInfo> infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Count; i++)
			{
				Apply(infos[i]);
			}
		}
	}

	public void Apply(ExpResultInfo info)
	{
		if (info != null)
		{
			SetTeamExp(info.NewValue, info.NewLevel);
			SetTeamLevel(info.NewLevel);
		}
	}

	public void Apply(IList<ExpResultInfo> infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Count; i++)
			{
				Apply(infos[i]);
			}
		}
	}

	public void Apply(GuildPointResultInfo info)
	{
		if (info != null)
		{
			SetGuildPoint(info.NewValue);
			SetDailyAccGuildPoint(info.NewDailyAccValue);
		}
	}

	public void Apply(RoyalPointResultInfo info)
	{
		if (info != null)
		{
			SetRoyalPoint(info.NewValue);
		}
	}

	public void Apply(IList<RoyalPointResultInfo> infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Count; i++)
			{
				Apply(infos[i]);
			}
		}
	}

	public void Apply(FriendshipPointResultInfo info)
	{
		if (info != null)
		{
			SetFriendshipPoint(info.NewValue);
			SetDailyAccFriendshipPoint(info.NewDailyAccValue);
		}
	}

	public void Apply(IList<FriendshipPointResultInfo> infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Count; i++)
			{
				Apply(infos[i]);
			}
		}
	}

	public void ApplyCurrency<T>(T info) where T : ICurrencyResultInfo
	{
		if (info != null)
		{
			SetUserCurrency(info.CurrencyType, info.NewValue);
		}
	}

	public void ApplyCurrency<T>(T[] infos) where T : ICurrencyResultInfo
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			ApplyCurrency(infos[i]);
		}
	}

	public void Apply(RaidResultInfo info)
	{
		if (info != null)
		{
			SetLastRaidResetTime(info.LastRaidResetTime);
		}
	}

	public void Apply(PlayerMiscInfo info)
	{
		if (info != null)
		{
			SetInventoryExtend(info.InventoryExtend);
			SetChestExtend(info.ChestExtend);
			SetPunishmentRuneExtend(info.PunishmentRuneStorageSlotExtend);
			SetLastRaidResetTime(info.LastRaidResetTime);
		}
	}

	private void SetGuildPoint(long guildPoint)
	{
		long guildPoint2 = _guildPointInfo.GuildPoint;
		_guildPointInfo.GuildPoint = guildPoint;
		if (guildPoint2 != _guildPointInfo.GuildPoint)
		{
			_observerManager.AnnounceToObservers(new UserStatInfo
			{
				UserStatType = UserStatType.GuildPoint,
				BeforeValue = guildPoint2,
				AfterValue = _guildPointInfo.GuildPoint
			});
		}
	}

	private void SetDailyAccGuildPoint(long dailyAccGuildPoint)
	{
		long dailyAccGuildPoint2 = _guildPointInfo.DailyAccGuildPoint;
		_guildPointInfo.DailyAccGuildPoint = dailyAccGuildPoint;
		if (dailyAccGuildPoint2 != _guildPointInfo.DailyAccGuildPoint)
		{
			_observerManager.AnnounceToObservers(new UserStatInfo
			{
				UserStatType = UserStatType.DailyAccGuildPoint,
				BeforeValue = dailyAccGuildPoint2,
				AfterValue = _guildPointInfo.DailyAccGuildPoint
			});
		}
	}

	public void SetPlayerAvaterHeroInfo(PlayerAvatarHeroInfo playerAvatarHeroInfo)
	{
		_playerAvatarHeroInfo = playerAvatarHeroInfo;
	}

	public void SetAvatarHeroIndex(int avatarIndex, PlayerAvatarHeroInfo playerAvatarHeroInfo)
	{
		int avatarHeroIndex = _userInfo.AvatarHeroIndex;
		_userInfo.AvatarHeroIndex = avatarIndex;
		PlayerAvatarHeroInfo playerAvatarHeroInfo2 = _playerAvatarHeroInfo;
		_playerAvatarHeroInfo = playerAvatarHeroInfo;
		if (avatarHeroIndex != avatarIndex)
		{
			_observerManager.AnnounceToObservers(new UserStatInfo
			{
				UserStatType = UserStatType.AvatarHeroIndex,
				BeforeValue = avatarHeroIndex,
				AfterValue = avatarIndex
			});
		}
		else if (!playerAvatarHeroInfo2.IsEqual(playerAvatarHeroInfo))
		{
			_observerManager.AnnounceToObservers(new UserStatInfo
			{
				UserStatType = UserStatType.PlayerAvatarHeroInfo
			});
		}
	}

	public void SetAvatarHero(int avatarIndex)
	{
		_observerManager.AnnounceToObservers(new UserStatInfo
		{
			UserStatType = UserStatType.SubStoryAvatarHeroIndex,
			AfterValue = avatarIndex
		});
	}

	public void SetOpenGuildTime(string openGuildTime)
	{
		MiscInfo.OpenGuildTime = openGuildTime;
	}

	public void SetInventoryExtend(int after)
	{
		int inventoryExtend = _miscInfo.InventoryExtend;
		_miscInfo.InventoryExtend = after;
		_observerManager.AnnounceToObservers(new UserStatInfo
		{
			UserStatType = UserStatType.InventoryExtend,
			BeforeValue = inventoryExtend,
			AfterValue = after
		});
	}

	public void SetChestExtend(int after)
	{
		int chestExtend = _miscInfo.ChestExtend;
		_miscInfo.ChestExtend = after;
		_observerManager.AnnounceToObservers(new UserStatInfo
		{
			UserStatType = UserStatType.InventoryExtend,
			BeforeValue = chestExtend,
			AfterValue = after
		});
	}

	public void SetPunishmentRuneExtend(int after)
	{
		int punishmentRuneStorageSlotExtend = _miscInfo.PunishmentRuneStorageSlotExtend;
		_miscInfo.PunishmentRuneStorageSlotExtend = after;
		_observerManager.AnnounceToObservers(new UserStatInfo
		{
			UserStatType = UserStatType.InventoryExtend,
			BeforeValue = punishmentRuneStorageSlotExtend,
			AfterValue = after
		});
	}

	public void SetLastEventDungeon(int chapterIndex, int dungeonIndex)
	{
		if (dungeonIndex > _miscInfo.LastEventDungeonIndex)
		{
			_miscInfo.LastEventChapterIndex = chapterIndex;
			_miscInfo.LastEventDungeonIndex = dungeonIndex;
		}
	}

	public void SetCountryFlagIndex(string countryCode)
	{
		if (String_Extension_Method.IsNullOrEmpty(countryCode))
		{
			_miscInfo.CountryCode = "VESPA";
		}
		else
		{
			_miscInfo.CountryCode = countryCode;
		}
	}

	public string GetCountryFlagCode()
	{
		if (String_Extension_Method.IsNullOrEmpty(_miscInfo.CountryCode))
		{
			return "VESPA";
		}
		return _miscInfo.CountryCode;
	}

	public void SetSoulStoneMileage(long mileage)
	{
		if (_miscInfo != null && _miscInfo.SoulStoneMileage != mileage)
		{
			_observerManager.AnnounceToObservers(new UserStatInfo
			{
				UserStatType = UserStatType.SoulWeaponMilige,
				BeforeValue = _miscInfo.SoulStoneMileage,
				AfterValue = mileage
			});
			_miscInfo.SoulStoneMileage = mileage;
		}
	}

	public void SetRestoreSoulStoneIndices(int[] soulStoneIndexs)
	{
		if (_miscInfo == null)
		{
			return;
		}
		if (soulStoneIndexs == null || soulStoneIndexs.Length == 0)
		{
			_miscInfo.RestoreSoulStoneIndices = string.Empty;
			return;
		}
		string text = string.Empty;
		for (int i = 0; i < soulStoneIndexs.Length; i++)
		{
			int num = soulStoneIndexs[i];
			text = ((!String_Extension_Method.IsNullOrEmpty(text)) ? $"{text},{num}" : num.ToString());
		}
		_miscInfo.RestoreSoulStoneIndices = text;
	}

	public void SetGuildSuppressSessionInfo(GuildSuppressSessionInfo info)
	{
		if (info != null)
		{
			_customMiscInfo.GuildSuppressSessionInfo = info;
		}
	}

	public GuildSuppressSessionInfo GetGuildSuppressSessionInfo()
	{
		return _customMiscInfo.GuildSuppressSessionInfo;
	}

	public void SetGuildArenaSessionInfo(GuildArenaSessionInfo[] infos)
	{
		if (_miscInfo != null && __Extension_Method_ICollection__.COUNT(infos) != 0)
		{
			_miscInfo.GuildArenaSessionInfo = infos[0];
		}
	}

	public int[] GetProjectileGroupIndexFromServer(BattleType battleType)
	{
		if (battleType == BattleType.Shakmeh)
		{
			return _miscInfo.ShakemehPassiveInfos;
		}
		return null;
	}

	public void SetShakmehBuffInfo(int[] infos)
	{
		_miscInfo.ShakemehPassiveInfos = infos;
	}

	public void RemoveShakmehBuffInfo()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_miscInfo.ShakemehPassiveInfos))
		{
			_miscInfo.ShakemehPassiveInfos = null;
		}
	}

	public void SetGuildArenaSessionState(GuildArenaSessionState state)
	{
		if (state != GuildArenaSessionState.None && _miscInfo != null && _miscInfo.GuildArenaSessionInfo != null)
		{
			_miscInfo.GuildArenaSessionInfo.State = state;
		}
	}

	public void SetValanceOptionRenew(ValanceRenewOptionInfo optionInfo)
	{
		_miscInfo.ValanceRenewOptionInfo = optionInfo;
	}

	public void SetValanceEnchant(ValanceEnchantOptionInfo optionInfo)
	{
		_miscInfo.ValanceEnchantOptionInfo = optionInfo;
	}

	public void SetValanceSkillRenew(ValanceRenewSkillInfo skillInfo)
	{
		_miscInfo.ValanceRenewSkillInfo = skillInfo;
	}

	public ValanceRenewOptionInfo GetValanceOptionRenew()
	{
		if (_miscInfo.ValanceRenewOptionInfo == null)
		{
			return null;
		}
		return _miscInfo.ValanceRenewOptionInfo;
	}

	public ValanceEnchantOptionInfo GetValanceEnchant()
	{
		if (_miscInfo.ValanceEnchantOptionInfo == null)
		{
			return null;
		}
		return _miscInfo.ValanceEnchantOptionInfo;
	}

	public ValanceRenewSkillInfo GetValanceSkillRenew()
	{
		if (_miscInfo.ValanceRenewSkillInfo == null)
		{
			return null;
		}
		return _miscInfo.ValanceRenewSkillInfo;
	}

	public void SetPlayerTreasureHouseInfo(PlayerTreasureHouseInfo playerTreasureHouseInfo)
	{
		if (playerTreasureHouseInfo != null)
		{
			_customMiscInfo.PlayerTreasureHouseInfo = playerTreasureHouseInfo;
		}
	}

	public void ApplyPlayerTreasureHouseInfo(PlayerTreasureHouseInfo playerTreasureHouseInfo)
	{
		if (playerTreasureHouseInfo != null)
		{
			SetPlayerTreasureHouseInfo(playerTreasureHouseInfo);
			_observerManager.AnnounceToObservers(new UserStatInfo
			{
				UserStatType = UserStatType.TreasureHousePoint
			});
		}
	}
}
