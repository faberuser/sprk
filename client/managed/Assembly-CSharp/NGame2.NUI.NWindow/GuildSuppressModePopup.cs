namespace NGame2.NUI.NWindow;

public class GuildSuppressModePopup : BaseWindow
{
	public interface Listener
	{
		void OnSelect(bool isMulti, int raidIndex, int raidLevel);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildSuppress/GuildSuppressModePopup";

	private int _raidIndex;

	private int _raidLevel;

	private Listener _listener;

	public void Open(int raidIndex, int raidLevel, Listener listener = null)
	{
		Open(delegate
		{
			Init(raidIndex, raidLevel, listener);
		}, null);
	}

	private void Init(int raidIndex, int raidLevel, Listener listener)
	{
		_listener = listener;
		_raidIndex = raidIndex;
		_raidLevel = raidLevel;
	}

	public void OnClickSingle()
	{
		if (_listener != null)
		{
			_listener.OnSelect(isMulti: false, _raidIndex, _raidLevel);
			Close();
		}
	}

	public void OnClickMulti()
	{
		if (_listener != null)
		{
			_listener.OnSelect(isMulti: true, _raidIndex, _raidLevel);
			Close();
		}
	}
}
