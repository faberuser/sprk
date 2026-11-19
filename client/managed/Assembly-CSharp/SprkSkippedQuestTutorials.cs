using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NLogin;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

// With the global tutorial skip, event-triggered tutorials never start. Some
// Guideline quests ("Enter Chapter 2", "Visit the Hall of Heroes", "Meet
// Veronica at the Guild", "Visit Valance's Workshop", ...) complete only when
// their tutorial is completed, so they could never finish. When the event that
// would have started such a tutorial happens, complete it without playing it.
public static class SprkSkippedQuestTutorials
{
	private static HashSet<int> targets;

	private static readonly HashSet<int> pending = new HashSet<int>();

	private static readonly HashSet<int> failed = new HashSet<int>();

	// Called by EventTriggerManager.CanStartTutorial when tutorials are disabled;
	// the event and the trigger's current condition have already matched.
	public static void Observe(EventTriggerData data)
	{
		if (data != null && data.Command == EventTriggerCommand.StartTutorial && __Extension_Method_ICollection__.COUNT(data.Argument) >= 1 && int.TryParse(data.Argument[0].Trim(), out var result))
		{
			Observe(result);
		}
	}

	// Called by TutorialManager.StartByEventTrigger (EventTriggerManager.RaiseEvent)
	// when tutorials are disabled.
	public static void Observe(int index)
	{
		try
		{
			if (index <= 0 || !GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return;
			}
			AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
			if (!instance.IsTutorialSkip || instance.TutorialManager == null || instance.TutorialManager.IsComplete(index) || !Targets().Contains(index) || failed.Contains(index) || pending.Contains(index))
			{
				return;
			}
			WebServiceManager instance2 = SingletonUpdater<WebServiceManager>.instance;
			if (instance2.Requester == null)
			{
				return;
			}
			pending.Add(index);
			instance2.Requester.Request(new NShared.CompleteTutorial.Request
			{
				TutorialIndex = index
			}, delegate(NShared.CompleteTutorial.Response response)
			{
				pending.Remove(index);
				Apply(index, response);
			});
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK skipped quest tutorial: " + ex.GetType().Name + ": " + ex.Message);
		}
	}

	// Tutorials that a Guideline (sub-quest) goal requires.
	private static HashSet<int> Targets()
	{
		if (targets != null)
		{
			return targets;
		}
		HashSet<int> hashSet = new HashSet<int>();
		IList<SubQuestData> list = GameRoot.Instance.SubQuestDataContainer?.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			SubQuestData subQuestData = list[i];
			if (subQuestData != null && subQuestData.StateType == QuestActionType.CompleteTutorial && __Extension_Method_ICollection__.COUNT(subQuestData.StateValue) > 0 && int.TryParse(subQuestData.StateValue[0].Trim(), out var result) && result > 0)
			{
				hashSet.Add(result);
			}
		}
		if (hashSet.Count > 0)
		{
			targets = hashSet;
		}
		return hashSet;
	}

	private static void Apply(int index, NShared.CompleteTutorial.Response response)
	{
		if (response == null || response.BaseResult != BaseResultType.Success || response.Result != NShared.CompleteTutorial.ResultType.Success)
		{
			failed.Add(index);
			return;
		}
		try
		{
			AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
			if (response.Info != null)
			{
				instance.TutorialManager.Add(response.Info);
			}
			instance.UserManager.Apply(response.CurrencyResults);
			instance.CampaignManager.Apply(response.DungeonInfos);
			instance.ItemManager.Apply(response.ItemResults);
			instance.EquipItemManager.Apply(response.EquipItemResults);
			instance.ApplyAddHeroForTutorial(response.HeroInfos, response.TeamExpResultInfos);
			instance.UserManager.Apply(response.StaminaResultInfos);
			instance.FreeEquipGachaManager.Set(response.FreeEquipGachaInfo);
			if (response.OpendMissionCategories != null)
			{
				LoginData.MissionCategoryInfo = response.OpendMissionCategories;
			}
			if (__Extension_Method_ICollection__.COUNT(response.ItemResults) > 0 || __Extension_Method_ICollection__.COUNT(response.EquipItemResults) > 0 || __Extension_Method_ICollection__.COUNT(response.CurrencyResults) > 0)
			{
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Tutorial.TUTORIAL_REWARD_TITLE)).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemResults)
					.SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: true);
			}
			if (SceneAutoBehaviour<MissionManagement>.isValidInstance)
			{
				SceneAutoBehaviour<MissionManagement>.instance.RefreshQuestList();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK skipped quest tutorial apply: " + ex.GetType().Name + ": " + ex.Message);
		}
	}
}
