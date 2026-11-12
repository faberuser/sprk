using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWorldMap;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NLobby.NTask;

public class MoveToNode : ILobbyTaskCreator
{
	public virtual ITask Create(string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		bool valueBool = ArgParser.GetValueBool(values, 2);
		return new DelegateTask(Run(valueInt, valueInt2, valueBool));
	}

	protected IEnumerator<TaskCurrentData> Run(int chapterIndex, int targetNodeIndex, bool runNodeAction)
	{
		int targetLastNodeIndex = targetNodeIndex;
		bool targetNodeCanVisit = true;
		while (targetLastNodeIndex > 1 && !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapterIndex, targetLastNodeIndex))
		{
			targetLastNodeIndex--;
			targetNodeCanVisit = false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapterIndex, targetLastNodeIndex))
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			yield return TaskCurrentData.FAIL;
		}
		bool moveStarted = false;
		SceneAutoBehaviour<WorldMapManagement>.instance.MovePlayerToDungeon(chapterIndex, targetLastNodeIndex, DisableTouch: false, warp: false, delegate
		{
			moveStarted = true;
		});
		while (!moveStarted)
		{
			yield return TaskCurrentData.RUNNING;
		}
		MapLayout mayLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		while (SceneAutoBehaviour<WorldMapManagement>.instance.IsMapLoading || mayLayout.IsPlayerMoving())
		{
			yield return TaskCurrentData.RUNNING;
		}
		if (runNodeAction)
		{
			if (NGame2.NAccount.UserCampaignManager.DungeonIndex == targetNodeIndex)
			{
				SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(null);
			}
			else if (!targetNodeCanVisit && NGame2.NAccount.UserCampaignManager.DungeonIndex == targetLastNodeIndex)
			{
				CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			}
		}
		yield return TaskCurrentData.SUCCESS;
	}
}
