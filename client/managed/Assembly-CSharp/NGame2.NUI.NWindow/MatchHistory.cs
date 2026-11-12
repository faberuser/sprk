using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NBattleField.NReplay;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MatchHistory : BaseWindow, MatchReplayComponent.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchHistory";

	public const int MaxReplayCount = 10;

	public UILabel Label_NoHistory;

	public UIScrollGrid HistoryGrid;

	private List<MatchReplayComponent> _componentList = new List<MatchReplayComponent>();

	private Replay[] _replays;

	private MatchPvpType _matchType;

	public void Init(Replay[] replays, MatchPvpType matchType)
	{
		_replays = replays;
		_matchType = matchType;
		RefreshList();
	}

	public override void Reset()
	{
		base.Reset();
		ClearList();
	}

	private void ClearList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public void RefreshList()
	{
		UIHelper.EnableGameObject(Label_NoHistory, _replays == null || _replays.Length == 0);
		if (_replays == null)
		{
			return;
		}
		ClearList();
		Replay[] replays = _replays;
		foreach (Replay replay in replays)
		{
			if (replay.Info != null)
			{
				ArenaReplayInfo arenaReplayInfo = JsonMarshaler.Parse<ArenaReplayInfo>(replay.Info);
				if (arenaReplayInfo != null)
				{
					MatchReplayComponent matchReplayComponent = UIHelper.AddGridItem(HistoryGrid, MatchReplayComponent.PrefabAuto);
					matchReplayComponent.Open(arenaReplayInfo, _matchType, replay.Uid, this);
					_componentList.Add(matchReplayComponent);
				}
			}
		}
		UIHelper.RepositionGrid(HistoryGrid);
	}

	void MatchReplayComponent.Listener.OnView(MatchReplayComponent caller)
	{
		if (SceneAutoBehaviour<MatchManagement>.isValidInstance)
		{
			SingletonAuto<ReplayManager>.instance.StartReplay(caller.ReplayUid, delegate(ArenaReplayInfo replayInfo, BaseBattleLog[] battleLogs)
			{
				replayInfo.ArenaType = _matchType.ToMatchArenaType();
				LobbyScene2.ListReturnState.Add(new MatchReturnState(_matchType));
				SingletonAuto<BattleContext>.instance.SetArenaReplay(replayInfo, battleLogs);
			});
		}
	}
}
