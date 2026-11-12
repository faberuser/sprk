using System;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class RaidJoinReturnState : LobbyScene2.IReturnState
{
	private int _raidIndex;

	private int _raidLevel;

	public string BgmCode => string.Empty;

	public RaidJoinReturnState(int raidIndex, int raidLevel)
	{
		_raidIndex = raidIndex;
		_raidLevel = raidLevel;
	}

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if (GameRoot.Instance.RaidDataContainer.GetData(_raidIndex, _raidLevel) == null)
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
			return;
		}
		SceneAutoBehaviour<RaidManagement>.instance.OpenRaidPartySetting(_raidIndex, _raidLevel);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
