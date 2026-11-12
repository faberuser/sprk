using System;
using NGame2.NScene.NPreloader;

namespace NGame2.NUI.NManager;

public class PrefabWrapper
{
	public IWindowPrefab Prefab;

	public bool IsPassiveWindow;

	public Func<IWindow> InstancingAction;

	public Action<UiPreloader> ReserveCacheAction;

	private IWindow _window;

	public IWindow Window => _window;

	public void Instantiate()
	{
		if (_window == null)
		{
			_window = InstancingAction();
		}
	}
}
