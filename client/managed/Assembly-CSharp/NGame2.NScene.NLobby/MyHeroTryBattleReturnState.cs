using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class MyHeroTryBattleReturnState : LobbyScene2.IReturnState
{
	public string BgmCode => string.Empty;

	public CachedHeroInfo HeroInfo { get; private set; }

	public MyHeroTryBattleReturnState(CachedHeroInfo heroInfo)
	{
		HeroInfo = heroInfo;
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<MyHeroManagement>.instance.RestoreHeroDetailView(HeroInfo, onEnd);
	}
}
