using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class PayShopHeroInfoPopupReturnState : LobbyScene2.IReturnState
{
	private int _heroIndex;

	public string BgmCode => string.Empty;

	public PayShopHeroInfoPopupReturnState(int heroIndex)
	{
		_heroIndex = heroIndex;
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<PayShopManagement>.instance.RestoreHeroInfoPopup(_heroIndex);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
