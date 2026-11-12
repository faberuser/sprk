using System;
using NGame2.NScene.NPreloader;
using UnityEngine;

namespace NGame2.NUI.NWindow.NLoading;

public class ManualLoading : MonoBehaviour, IGlobalWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Loading/ManualLoading";

	private static GlobalWindowPrefab<ManualLoading> _prefabInstance = new GlobalWindowPrefab<ManualLoading>("Resources/Prefab2/Window/Loading/ManualLoading", GlobalUIRoot.ELayerType.GlobalUI);

	private Action _onComplete;

	public static ManualLoading instance => _prefabInstance.instance;

	public static bool isValidInstance => _prefabInstance.IsValidInstance;

	public static void CreateInstance()
	{
		_prefabInstance.CreateInstance();
	}

	public static void ReleaseInstance()
	{
		_prefabInstance.ReleaseWindow();
	}

	public void Open(Action onComplete)
	{
		_onComplete = onComplete;
	}

	private void Awake()
	{
	}

	private void Update()
	{
	}

	public void OnClickComplete()
	{
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
	}

	void IGlobalWindow.ReserveCache(UiPreloader preloader)
	{
	}

	public bool IsOpen()
	{
		return true;
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}
}
