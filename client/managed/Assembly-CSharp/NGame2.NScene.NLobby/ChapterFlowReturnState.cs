using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWorldMap.NMapLayout;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class ChapterFlowReturnState : LobbyScene2.IReturnState
{
	private int _chapterIndex;

	private int _dungeonIndex;

	private bool _alreadyCompletedDungeon;

	public string BgmCode => string.Empty;

	public ChapterFlowReturnState(int chapterIndex, int dungeonInex)
	{
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonInex;
		_alreadyCompletedDungeon = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_chapterIndex, _dungeonIndex);
	}

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if (_alreadyCompletedDungeon)
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
			return;
		}
		SceneAutoBehaviour<WorldMapManagement>.instance.ProcessChapterFlow(_chapterIndex, _dungeonIndex, ChapterFlowManager.ConditionKeyType.CompleteDungeon);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
