using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

public static class SprkAdventureUnlocks
{
	public static bool TutorialSatisfied(bool skipped, NShared.UserTutorialManager manager, int index)
	{
		if (!skipped)
		{
			return manager?.IsComplete(index) ?? false;
		}
		return true;
	}

	public static bool PortalTutorial(NShared.UserTutorialManager manager, int index)
	{
		return TutorialSatisfied(GlobalAutoBehaviour<AccountManager>.instance.IsTutorialSkip, manager, index);
	}

	public static bool IsAutomaticDungeonQuest(SubQuestData quest)
	{
		if (quest != null && quest.Step == 1 && quest.AutoComplete && !quest.ClientOnly && quest.StateType == QuestActionType.DungeonCleared && quest.StateValue != null && quest.StateValue.Length == 2 && int.TryParse(quest.StateValue[0], out var result) && result > 0 && int.TryParse(quest.StateValue[1], out var result2) && result2 > 0)
		{
			return quest.ReqProgress <= 1;
		}
		return false;
	}

	public static bool MissionCategory(bool serverOpen, int main, int sub)
	{
		if (serverOpen)
		{
			return true;
		}
		GameRoot instance = GameRoot.Instance;
		MissionCategoryData data = instance.MissionCategoryDataContainer.GetData(main, sub);
		if (data == null || data.OpenConditionType != "ClearQuestIndex" || data.OpenConditionValue == null || data.OpenConditionValue.Length != 2 || data.OpenConditionValue[0] != "SubQuest" || !int.TryParse(data.OpenConditionValue[1], out var result))
		{
			return false;
		}
		SubQuestData subQuestData = instance.SubQuestDataContainer.GetSubQuestData(result);
		if (!IsAutomaticDungeonQuest(subQuestData))
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(int.Parse(subQuestData.StateValue[0]), int.Parse(subQuestData.StateValue[1]));
	}
}
