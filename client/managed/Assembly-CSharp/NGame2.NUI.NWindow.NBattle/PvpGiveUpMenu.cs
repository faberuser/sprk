using System;

namespace NGame2.NUI.NWindow.NBattle;

public class PvpGiveUpMenu : BaseWindow
{
	public interface Listener
	{
		void OnGiveUp(Action<bool> onComplete);

		void OnClose();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/PvpGiveUpMenu";

	private Listener _listener;

	public bool IsPending { get; private set; }

	public bool RequestSuccess { get; private set; }

	public void Open(Listener listener)
	{
		Open(delegate
		{
			Init(listener);
		}, null);
	}

	public void Init(Listener listener)
	{
		_listener = listener;
		RequestSuccess = false;
		IsPending = false;
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnClickClose()
	{
		if (_listener != null)
		{
			_listener.OnClose();
		}
	}

	public void OnClickGiveUp()
	{
		if (_listener != null)
		{
			IsPending = true;
			_listener.OnGiveUp(delegate(bool isSuccess)
			{
				IsPending = false;
				RequestSuccess = isSuccess;
			});
		}
	}
}
