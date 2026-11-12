using System;
using NGame2.NMatch;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class MatchReturnState : BaseEntryMenuReturnState
{
	private MatchPvpType _matchType;

	private bool _retry;

	private MatchResultInfo _lastMatchResultInfo;

	public string BgmCode => string.Empty;

	public MatchReturnState(MatchPvpType matchType, bool retry = false, MatchResultInfo lastMatchResultInfo = null)
	{
		_matchType = matchType;
		_retry = retry;
		_lastMatchResultInfo = lastMatchResultInfo;
	}

	public override void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<MatchManagement>.instance.OpenMatchType(_matchType, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
			if (_retry && _lastMatchResultInfo != null)
			{
				SingletonUpdater<MatchManager>.instance.RetryMatch_Pvp(_lastMatchResultInfo);
			}
		});
	}
}
