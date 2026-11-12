using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenSubStory : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		ArgParser.GetValueInt(menu.EntryMenuButton, 2);
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			VespaDebug.LogError($"OpenSubStory :: {chapterIndex}-{dungeonIndex} entryNode {data.ContentsType}");
			SceneAutoBehaviour<SubStoryManagement>.instance.SetEntryNode(data);
		}
		SceneAutoBehaviour<SubStoryManagement>.instance.OpenSubStoryCharacterScene();
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}
