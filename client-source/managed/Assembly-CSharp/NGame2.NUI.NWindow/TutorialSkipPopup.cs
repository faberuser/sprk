using System;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class TutorialSkipPopup : BaseWindow
{
	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UIScrollView ScrollView;

	private Action _onClose;

	public static void Open(string prefabPath, Action onClose, string title = "", string message = "")
	{
		try
		{
			WindowPrefab<TutorialSkipPopup> windowPrefab = new WindowPrefab<TutorialSkipPopup>(prefabPath, GameUIRoot.ELayerType.UI);
			windowPrefab.instance.OpenInternal(onClose, title, message);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
	}

	protected void OpenInternal(Action onClose, string title, string message)
	{
		base.Open();
		_onClose = onClose;
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		if (ScrollView != null)
		{
			ScrollView.ResetPosition();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Vespa_System_Action_Extension_Method.fire(_onClose);
		_onClose = null;
	}

	public void OnClickGuide()
	{
		base.Close();
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.Tutorial_OpenGameGuideWindow();
		}
	}
}
