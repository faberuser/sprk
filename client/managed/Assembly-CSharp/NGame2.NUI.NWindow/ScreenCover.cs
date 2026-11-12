using NGame2.NScene.NPreloader;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ScreenCover : MonoBehaviour, IWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/ScreenCover";

	private static WindowPrefab<ScreenCover> _windowPrefab = new WindowPrefab<ScreenCover>("Resources/Prefab2/Window/ScreenCover", GameUIRoot.ELayerType.UI);

	public UIWidget BlackWidget;

	EDepthOrder IWindow.DepthOrder => EDepthOrder.Normal;

	public static void Open()
	{
		_windowPrefab.instance.OpenInternal();
	}

	public static void Close()
	{
		_windowPrefab.CloseWindow();
	}

	internal void OpenInternal()
	{
		base.gameObject.SetActive(value: true);
	}

	void IWindow.ReserveCache(UiPreloader preloader)
	{
	}

	void IWindow.SetBasePanelDepth(int depth)
	{
	}

	bool IWindow.IsOpen()
	{
		return base.gameObject.activeSelf;
	}

	void IWindow.Close()
	{
		base.gameObject.SetActive(value: false);
	}
}
