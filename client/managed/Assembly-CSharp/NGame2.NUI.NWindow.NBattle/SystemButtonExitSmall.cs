namespace NGame2.NUI.NWindow.NBattle;

public class SystemButtonExitSmall : BaseWindow
{
	public interface Listener
	{
		void OnExit();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/SystemButtonExitSmall";

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
	}

	public void OnClickExit()
	{
		if (_listener != null)
		{
			_listener.OnExit();
		}
	}
}
