using NVespa;

namespace NGame2.NUI.NWindow.NBattle;

public class EclipsePausedMenu : BaseWindow
{
	public interface Listener
	{
		void OnRetry();

		void OnReturnToMap();

		void OnClose();

		void OnNextTeam();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/EclipsePausedMenu";

	public UIButton RetryButton;

	private Listener _listener;

	public void Open(Listener listener, bool enableRetry)
	{
		Open(delegate
		{
			Init(listener, enableRetry);
		}, null);
	}

	public void Init(Listener listener, bool enableRetry)
	{
		_listener = listener;
		UIHelper.EnableButton(RetryButton, enableRetry);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.Menu);
	}

	public override void OnSystemBackButton()
	{
		OnClickBack();
	}

	public void OnClickRetry()
	{
		if (_listener != null)
		{
			_listener.OnRetry();
		}
	}

	public void OnClickReturnToMap()
	{
		if (_listener != null)
		{
			_listener.OnReturnToMap();
		}
	}

	public void OnClickBack()
	{
		if (_listener != null)
		{
			_listener.OnClose();
		}
	}

	public void OnClickNextTeam()
	{
		if (_listener != null)
		{
			_listener.OnNextTeam();
		}
	}
}
