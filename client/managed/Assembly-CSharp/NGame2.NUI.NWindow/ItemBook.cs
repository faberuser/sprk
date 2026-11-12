namespace NGame2.NUI.NWindow;

public class ItemBook : BaseWindow
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/ItemBook";

	private Listener _listener;

	public void Open(Listener listener)
	{
		_listener = listener;
		base.Open(delegate
		{
			Init();
		});
	}

	public void Init()
	{
	}
}
