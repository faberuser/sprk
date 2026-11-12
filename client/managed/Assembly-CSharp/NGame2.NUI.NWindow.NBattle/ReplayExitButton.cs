using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow.NBattle;

public class ReplayExitButton : BaseWindow
{
	public interface Listener
	{
		void OnExit();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/ReplayExitButton";

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
	}

	public void OnClickExit()
	{
		GlobalTimer.PauseTimer(GlobalTimer.EPauseFlag.Menu);
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(Battle.EXIT_REPLAY_CONFIRM);
		MessagePopup.Open(text, message, delegate
		{
			GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.Menu);
			if (_listener != null)
			{
				_listener.OnExit();
			}
		}, delegate
		{
			GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.Menu);
		});
	}

	public override void OnSystemBackButton()
	{
		OnClickExit();
	}
}
