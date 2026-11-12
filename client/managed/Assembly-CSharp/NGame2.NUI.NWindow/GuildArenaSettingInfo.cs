using System;

namespace NGame2.NUI.NWindow;

public class GuildArenaSettingInfo : BaseWindow
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaSettingInfo";

	private Listener _listener;

	public void Open(Listener listener, Action onComplete = null)
	{
		_listener = listener;
		Open(Init, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		});
	}

	private void Init()
	{
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void Close()
	{
		base.Close();
	}
}
