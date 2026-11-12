namespace NGame2.NUI.NWindow.NBattle;

public class SystemButtonPause : BaseWindow
{
	public interface Listener
	{
		void OnPause();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/SystemButtonPause";

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
	}

	public void OnClickPause()
	{
		if (_listener != null)
		{
			_listener.OnPause();
		}
	}
}
