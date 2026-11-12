using System;
using NGame2.NMatch;
using NVespa;

namespace NGame2.NScene.NLobby;

public class AcceptDuelReturnState : LobbyScene2.IReturnState
{
	private long _requestId;

	private long _groupId;

	public string BgmCode => string.Empty;

	public AcceptDuelReturnState(long requestId, long groupId)
	{
		_requestId = requestId;
		_groupId = groupId;
	}

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SingletonUpdater<DuelManager>.instance.AcceptDuel(_requestId, _groupId);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
