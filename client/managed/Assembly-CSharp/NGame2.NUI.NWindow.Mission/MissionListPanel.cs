using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.Mission;

public class MissionListPanel : MonoBehaviour
{
	public QuestList QuestListPanel;

	public void Init()
	{
		InitQuestListPanel();
	}

	private void InitQuestListPanel()
	{
		if (!(QuestListPanel == null))
		{
			QuestListPanel.Init();
		}
	}

	public void SelectCategory(int mainCategoryIndex, int subCategoryIndex = 0)
	{
		SelectMainCategory(mainCategoryIndex, subCategoryIndex);
	}

	private void SelectListCategory(QuestList.EListType listType, int mainCategoryIndex)
	{
		if (!(QuestListPanel == null))
		{
			QuestListPanel.SelectMainCategory(listType, mainCategoryIndex);
		}
	}

	private void SelectMainCategory(int mainCategoryIndex, int subCategoryIndex = 0)
	{
		switch (EnumTools.Parse(GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(mainCategoryIndex).UIPropName, EUIPropType.Null))
		{
		case EUIPropType.Main:
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionNode2Window(mainCategoryIndex, subCategoryIndex);
			break;
		case EUIPropType.Node:
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionNodeWindow(mainCategoryIndex, subCategoryIndex);
			break;
		case EUIPropType.Node3:
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionNodeWindowV3(mainCategoryIndex, subCategoryIndex);
			break;
		case EUIPropType.List1:
			SelectListCategory(QuestList.EListType.List1, mainCategoryIndex);
			break;
		case EUIPropType.List2:
			SelectListCategory(QuestList.EListType.List2, mainCategoryIndex);
			break;
		case EUIPropType.Hero:
			if (subCategoryIndex > 0)
			{
				SceneAutoBehaviour<AchievementManagement>.instance.RestoreHeroDetailView(subCategoryIndex);
			}
			else
			{
				SceneAutoBehaviour<AchievementManagement>.instance.OpenAchievementList();
			}
			break;
		}
	}
}
