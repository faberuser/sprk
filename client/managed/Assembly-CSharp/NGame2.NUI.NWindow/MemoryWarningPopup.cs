namespace NGame2.NUI.NWindow;

public class MemoryWarningPopup : BaseWindow
{
	public interface Listener
	{
		void OnOpenedMemoryWarningPopup();

		void OnClosedMemoryWarningPopup();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/MemoryWarningPopup";

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		if (_listener != null)
		{
			_listener.OnOpenedMemoryWarningPopup();
		}
	}

	public void OnClickConfirm()
	{
		Close();
		if (_listener != null)
		{
			_listener.OnClosedMemoryWarningPopup();
		}
	}
}
