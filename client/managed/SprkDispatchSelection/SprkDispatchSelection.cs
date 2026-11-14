using System.Linq;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NShared.RegisterMatch;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkDispatchSelection
{
	private static readonly MethodInfo ForcedHero = typeof(BattlePartySetting).GetMethod("IsForcedHero", BindingFlags.Instance | BindingFlags.NonPublic);

	public static bool IsDispatched(int hero)
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		UserDispatchBattleInfoManager userDispatchBattleInfoManager = ((instance == null) ? null : instance.DispatchBattleInfoManager);
		if (userDispatchBattleInfoManager != null)
		{
			return userDispatchBattleInfoManager.GetByHeroIndex(hero) != null;
		}
		return false;
	}

	public static int[] AvailableHeroes(int[] heroes)
	{
		return heroes?.Where((int id) => !IsDispatched(id)).ToArray();
	}

	public static bool CanUseHeroes(int[] heroes)
	{
		if (heroes == null || !heroes.Any(IsDispatched))
		{
			return true;
		}
		MessagePopup.Open(SprkText.Get("Hero dispatched"), SprkText.Get("A selected Hero is dispatched and cannot enter this battle. Select another Hero or cancel the dispatch first."));
		return false;
	}

	public static bool CanRegister(NShared.RegisterMatch.Request request)
	{
		if (request != null && request.DuelType != MatchDuelType.Friendly)
		{
			return CanUseHeroes(request.HeroIndices);
		}
		return true;
	}

	public static bool CanUseParty(BattlePartySetting window)
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		long mine = ((instance.UserInfo == null) ? 0 : instance.UserInfo.AccountId);
		return CanUseHeroes((from a in window.GetAllSquardHeroPortraitAnimList()
			where a != null && a.Component != null
			select a.Component into p
			where p.HeroInfo != null && (p.OwnerId == 0L || p.OwnerId == mine) && !(bool)ForcedHero.Invoke(window, new object[1] { p.HeroInfo.HeroIndex })
			select p.HeroInfo.HeroIndex).ToArray());
	}

	public static bool CanClick(HeroPortraitComponent portrait)
	{
		if (!portrait.IsDispatched || portrait.HeroInfo == null)
		{
			return true;
		}
		BattlePartySetting componentInParent = portrait.GetComponentInParent<BattlePartySetting>();
		if (!(componentInParent == null) && componentInParent.UserHeroListPanel != null)
		{
			return componentInParent.UserHeroListPanel.GetUserHeroPortraitComponent(portrait.OwnerId, portrait.HeroInfo.HeroIndex) != portrait;
		}
		return true;
	}

	public static void RefreshList(UserHeroListPanel panel)
	{
		panel.SetDispatch();
	}

	public static void FitList(UserHeroListPanel panel)
	{
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		long mine = ((instance.UserInfo == null) ? 0 : instance.UserInfo.AccountId);
		panel.ForeachAction(delegate(HeroPortraitComponent p)
		{
			if (!(p == null) && p.HeroInfo != null)
			{
				BattlePartySetting componentInParent = p.GetComponentInParent<BattlePartySetting>();
				bool flag = p.OwnerId != 0L && p.OwnerId != mine;
				bool flag2 = componentInParent != null && (bool)ForcedHero.Invoke(componentInParent, new object[1] { p.HeroInfo.HeroIndex });
				if (p.IsDispatched && (flag || flag2))
				{
					p.SetDispatched(isDispatched: false);
				}
				RestoredPortal.FitHeroDeckDispatchLabel(p);
			}
		});
	}

	public static void RefreshOpenLists()
	{
		BattlePartySetting[] array = Object.FindObjectsOfType<BattlePartySetting>();
		foreach (BattlePartySetting battlePartySetting in array)
		{
			if (battlePartySetting.UserHeroListPanel != null)
			{
				battlePartySetting.UserHeroListPanel.SetDispatch();
			}
		}
	}
}
