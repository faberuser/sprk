using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MatchMenu : BaseWindow, LobbyTopmostMenu.ICustomStaminaObserver
{
	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchMenu";

	public StaminaType StaminaType => StaminaType.Sword;

	public override void Reset()
	{
		base.Reset();
	}

	public void OnClickClose()
	{
		if (SceneAutoBehaviour<MatchManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MatchManagement>.instance.RequestCloseFromMatchMenu();
		}
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public override void ReOpen()
	{
		if (SceneAutoBehaviour<MatchManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MatchManagement>.instance.ReOpenComponent();
		}
	}

	public override void Close()
	{
		base.Close();
		MatchSelect instance = SceneAutoBehaviour<MatchManagement>.instance.GetInstance<MatchSelect>(openedOnly: true);
		if (instance != null)
		{
			instance.InitBtn();
		}
	}
}
