using System;
using NGame2.NParty;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NParty;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class MultiReturnState : LobbyScene2.IReturnState
{
	public PartyDungeonData _partyDungeonData;

	public string BgmCode => string.Empty;

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if (GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
			if (GlobalAutoBehaviour<PartyManager>.instance.IsPartyRoomMaster())
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendMultiBattleEndToServer();
			}
			GlobalAutoBehaviour<PartyManager>.instance.ChangeMemberStatus(MemberStatus.PartyRoom);
			GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.OnReturnRoom();
			SceneAutoBehaviour<MultiPartyManagement>.instance.OpenMultiPartySetting(partyInfo.ChapterIndex, partyInfo.DungeonIndex, partyInfo.Difficulty);
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}
	}
}
