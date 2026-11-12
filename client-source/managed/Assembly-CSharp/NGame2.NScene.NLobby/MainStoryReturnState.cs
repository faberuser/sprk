using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class MainStoryReturnState : LobbyScene2.IReturnState
{
	private EnterDungeonData _enterDungeon;

	private MainStoryChapterData _enterChapterData;

	private MainStoryData _enterStoryData;

	public string BgmCode => string.Empty;

	public MainStoryReturnState(EnterDungeonData enterDungeon, MainStoryChapterData enterChapterData, MainStoryData enterStoryData)
	{
		_enterDungeon = enterDungeon;
		_enterChapterData = enterChapterData;
		_enterStoryData = enterStoryData;
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(delegate
		{
			SceneAutoBehaviour<MainStoryManagement>.instance.ReturnToLastStory(_enterChapterData, _enterStoryData);
		});
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
