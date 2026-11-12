namespace NGame2.NUI.NWindow.NBattle;

public class BattleLeave : BaseWindow
{
	public interface Listener
	{
		void OnLeave();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BattleLeave";

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
	}

	public void OnClickButton()
	{
		if (_listener != null)
		{
			_listener.OnLeave();
		}
	}
}
