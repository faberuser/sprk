using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NTutorial;
using NGame2.NUI.NWorldMap;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class WorldMapPlayerMove : ITutorialRuntime, IEventRuntime, ICustomTutorialSkippable
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		yield return VespaCoroutine.Start(RunInternal(window, tutorialData, skip: false));
	}

	private IEnumerator RunInternal(TutorialWindow window, TutorialData tutorialData, bool skip)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 2);
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		if (argInt == chapterIndex && argInt2 == dungeonIndex)
		{
			instance.MapViewer.ReleaseFocusNode(null);
			yield break;
		}
		if (argInt != chapterIndex)
		{
			yield return VespaCoroutine.Start(coMoveChpater(argInt, argInt2));
		}
		else if (skip)
		{
			yield return VespaCoroutine.Start(coMovePlayerToDungeon(argInt, argInt2, warp: true));
		}
		else if (argBool)
		{
			yield return VespaCoroutine.Start(coMoveChpater(argInt, argInt2));
		}
		else
		{
			yield return VespaCoroutine.Start(coMovePlayerToDungeon(argInt, argInt2, warp: false));
		}
		yield return null;
		yield return null;
	}

	private IEnumerator coMoveChpater(int chapterIndex, int dungeonIndex)
	{
		bool isComplete = false;
		SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(chapterIndex, dungeonIndex, delegate
		{
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
	}

	private IEnumerator coMovePlayerToDungeon(int chapterIndex, int dungeonIndex, bool warp)
	{
		if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapterIndex, dungeonIndex))
		{
			yield break;
		}
		MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		if (!(mapLayout == null))
		{
			SceneAutoBehaviour<WorldMapManagement>.instance.IsPossibleMove = true;
			mapLayout.MovePlayerToDungeon(chapterIndex, dungeonIndex, warp);
			yield return null;
			yield return null;
			while (dungeonIndex != NGame2.NAccount.UserCampaignManager.DungeonIndex || mapLayout.IsPlayerMoving() || SceneAutoBehaviour<WorldMapManagement>.instance.IsMapLoading)
			{
				yield return null;
			}
		}
	}

	IEnumerator ICustomTutorialSkippable.Skip(TutorialWindow window, TutorialData tutorialData)
	{
		yield return VespaCoroutine.Start(RunInternal(window, tutorialData, skip: true));
	}
}
