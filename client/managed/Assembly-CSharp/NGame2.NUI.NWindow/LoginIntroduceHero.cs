using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class LoginIntroduceHero : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginIntroduceHero";

	private Action _onClose;

	public void Open(Action onComplete, Action onClose)
	{
		_onClose = onClose;
		base.Open(onComplete);
	}

	public override void OnSystemBackButton()
	{
		Vespa_System_Action_Extension_Method.fire(_onClose);
		base.OnSystemBackButton();
	}

	public void OnClickCancel()
	{
		OnSystemBackButton();
	}

	public void OnClickIntroduceWorld()
	{
		LoginIntroduceWorldPopup instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceWorldPopup>();
		if (instance != null && !instance.IsOpen())
		{
			instance.Open(null, null);
			instance.Init();
		}
		Vespa_System_Action_Extension_Method.fire(_onClose);
		base.Close();
	}

	public void OnClickIntroduceHero()
	{
		LoginIntroduceHeroPopup instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceHeroPopup>();
		if (instance != null && !instance.IsOpen())
		{
			instance.Open(null, null);
			instance.Init();
		}
		Vespa_System_Action_Extension_Method.fire(_onClose);
		base.Close();
	}

	public void OnClickIntroduceMiniGame()
	{
		LoginMiniGame.OpenPopup(openAtPatchDownload: false);
		Vespa_System_Action_Extension_Method.fire(_onClose);
		base.Close();
	}
}
