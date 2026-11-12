using System;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NOrdeal;

public static class OrdealHelper
{
	private static StringBuilder _sb = new StringBuilder(20);

	private const string TimeFormat = "{0:D2}:{1:D2}:{2:D2}";

	private const string MatchTierKeyFormat = "PVP_TIER_{0}";

	public static bool CheckOpenCondition()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.TutorialManager == null)
		{
			return false;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("OrdealArenaOpenCondition", 2130);
		return SprkAdventureUnlocks.PortalTutorial(GlobalAutoBehaviour<AccountManager>.instance.TutorialManager, valueInt);
	}

	public static void OpenAreaRewardPopup(RewardResultInfo resultInfo, Action onClose)
	{
		if (resultInfo != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("ORDEALARENA_MESSAGE_CLEARREWARD");
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(stringDirect).SetCurrencyResultInfos3(resultInfo.CurrencyResults).SetEquipResultInfos(resultInfo.EquipItemResults)
				.SetItemResultInfos(resultInfo.ItemResults)
				.SetStaminaResultInfos(resultInfo.StaminaResultInfos)
				.AddPetInfos(resultInfo.PetInfos), isGlobal: false, onClose);
		}
	}

	public static float GetEnterCameraDuration()
	{
		return (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("OrdealArenaEnterCameraTime", 100) / 100f;
	}

	public static int GetMaxRefreshNodeCount()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("OrdealArenaChangeOpponent", 5);
	}

	public static int GetResurrectionHeroGemCount()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("OrdealArenaRetireCharacterResetGem", 30);
	}

	public static int GetRefreshNodeGemCount()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("OrdealArenaChangeOpponentGem", 100);
	}

	public static string GetTierText(MatchTierType tierType)
	{
		_sb.Length = 0;
		if (tierType == MatchTierType.Challenger)
		{
			_sb.AppendFormat("PVP_TIER_{0}", "CHALLENGE");
		}
		else
		{
			_sb.AppendFormat("PVP_TIER_{0}", tierType.ToString().ToUpper());
		}
		return LocalizationManager.GetStringDirect(_sb.ToString());
	}

	public static string GetRemainTimeText(TimeSpan ts)
	{
		_sb.Length = 0;
		_sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
		return _sb.ToString();
	}

	public static MatchTierType GetMaxMatchTierType()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("PlayRecordMaxTierIndex", 402);
		PlayRecordInfo playRecordInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayRecordManager.Get(valueInt);
		if (playRecordInfo == null)
		{
			return MatchTierType.Bronze;
		}
		int num = Vespa_String_Extension_Method.TOINT(playRecordInfo.RecordValue, 0);
		if (num == GameRoot.Instance.ConstantDataContainer.GetValueInt("ChampionTierIndex"))
		{
			return MatchTierType.Challenger;
		}
		return GameRoot.Instance.MatchTierDataContainer.GetData(num)?.Tier ?? MatchTierType.Bronze;
	}

	public static int GetMatchTierIndex(MatchTierType matchTierType)
	{
		List<MatchTierData> tierDataListOfType = GameRoot.Instance.MatchTierDataContainer.GetTierDataListOfType(matchTierType);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(tierDataListOfType))
		{
			return GameRoot.Instance.MatchTierDataContainer.GetFirstData()?.Index ?? 0;
		}
		return tierDataListOfType[0].Index;
	}

	public static MatchedAccountInfo MyMatchAccountInfo(int[] heroIndices)
	{
		int matchTierIndex = GetMatchTierIndex(GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedTier());
		int matchScore = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetMatchScore();
		PlayerBattleInfo playerBattleInfo = new PlayerBattleInfo
		{
			TierIndex = matchTierIndex,
			WorldTierIndex = matchTierIndex,
			MatchScore = matchScore
		};
		CachedUserInfo userInfo = new CachedUserInfo
		{
			AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId,
			TeamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.TeamLevel,
			Nick = GlobalAutoBehaviour<AccountManager>.instance.Nick,
			AvatarHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex,
			TierIndex = matchTierIndex,
			MatchScore = playerBattleInfo.MatchScore,
			SeasonWin = playerBattleInfo.SeasonWin,
			SeasonLose = playerBattleInfo.SeasonLose,
			SuccessiveWin = playerBattleInfo.SuccessiveWin,
			IsOnline = true
		};
		Dictionary<int, CachedHeroInfo> dictionary = new Dictionary<int, CachedHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroIndices); i++)
		{
			int num = heroIndices[i];
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(num);
			if (heroInfo != null)
			{
				dictionary.Add(num, heroInfo);
			}
		}
		return new MatchedAccountInfo
		{
			ArenaType = MatchArenaType.Ordeal,
			TeamType = TeamType.Left,
			Rank = 1,
			BattleInfo = playerBattleInfo,
			UserInfo = userInfo,
			HeroInfos = dictionary
		};
	}

	public static MatchedAccountInfo NpcMatchAccountInfo(OrdealNodeInfo nodeInfo)
	{
		MatchTierType selectedTier = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSelectedTier();
		PlayerBattleInfo battleInfo = new PlayerBattleInfo
		{
			MatchScore = nodeInfo.AccountInfo.UserInfo.MatchScore,
			SeasonWin = nodeInfo.AccountInfo.UserInfo.SeasonWin,
			SeasonLose = nodeInfo.AccountInfo.UserInfo.SeasonLose,
			TierIndex = GetMatchTierIndex(selectedTier)
		};
		return new MatchedAccountInfo
		{
			ArenaType = MatchArenaType.Ordeal,
			TeamType = TeamType.Left,
			Rank = nodeInfo.Rank,
			IsNpc = true,
			BattleInfo = battleInfo,
			UserInfo = nodeInfo.AccountInfo.UserInfo,
			HeroInfos = nodeInfo.AccountInfo.HeroInfos
		};
	}

	public static bool SprkSyncDeadPortrait(HeroPortraitComponent portrait)
	{
		bool flag = GlobalAutoBehaviour<OrdealArenaManager>.instance.IsDeadHero(portrait.HeroInfo.HeroIndex);
		portrait.SetDead(flag);
		portrait.SetReviveGemCount(GetResurrectionHeroGemCount());
		return flag;
	}

	public static long SprkTotalGem(UserInfo user)
	{
		return checked(user.Gem + user.PayGem);
	}
}
