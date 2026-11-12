using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class MoveToFunctionNode : IActivity
{
	private NodeFunctionType _functionType;

	private Type _actionType;

	private bool _executeButtonAction;

	public MoveToFunctionNode(NodeFunctionType functionType, Type actionType, bool ExecuteButtonAction)
	{
		_functionType = functionType;
		_actionType = actionType;
		_executeButtonAction = ExecuteButtonAction;
	}

	IEnumerator IActivity.coStart()
	{
		List<CampaignNodeData> allDataOfFunctionType = GameRoot.Instance.CampaignNodeDataContainer.GetAllDataOfFunctionType(_functionType);
		if (__Extension_Method_ICollection__.COUNT(allDataOfFunctionType) == 0)
		{
			yield break;
		}
		int chapterIndex = allDataOfFunctionType[0].ChapterIndex;
		int dungeonIndex = allDataOfFunctionType[0].DungeonIndex;
		int chapterIndex2 = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex2 = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		if (chapterIndex2 != chapterIndex || dungeonIndex2 != dungeonIndex)
		{
			bool moveNodeCompleted = false;
			SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(chapterIndex, dungeonIndex, delegate
			{
				moveNodeCompleted = true;
			});
			while (!moveNodeCompleted)
			{
				yield return null;
			}
		}
		yield return null;
		bool isComplete = false;
		SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(delegate
		{
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
		SceneAutoBehaviour<EntryMenuManagement>.instance.ExecuteEntryMenuButtonAction(_actionType, _executeButtonAction);
	}

	void IActivity.OnRequestStop()
	{
	}
}
