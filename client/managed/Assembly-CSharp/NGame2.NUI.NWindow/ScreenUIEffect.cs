using System.Collections.Generic;
using NGame2.NScene.NPreloader;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ScreenUIEffect : MonoBehaviour, IWindow
{
	public const string PrefabPath = "Resources/Prefab2/ScreenUIEffect/ScreenUIEffect";

	private static IncreaseDic<GameUIRoot.ELayerType, WindowPrefab<ScreenUIEffect>> _gameUiDic = new IncreaseDic<GameUIRoot.ELayerType, WindowPrefab<ScreenUIEffect>>();

	public Transform Root;

	public string EffectPath;

	private List<GameObject> EffectList = new List<GameObject>();

	EDepthOrder IWindow.DepthOrder => EDepthOrder.Topmost;

	public GameObject LastEffect { get; private set; }

	public static ScreenUIEffect PlayEffect(string prefabName)
	{
		return PlayEffect(prefabName, GameUIRoot.ELayerType.UI);
	}

	public static ScreenUIEffect PlayEffect(string prefabName, GameUIRoot.ELayerType layerType)
	{
		WindowPrefab<ScreenUIEffect> windowPrefab = _gameUiDic.GETVALUE(layerType, null);
		if (windowPrefab == null)
		{
			windowPrefab = new WindowPrefab<ScreenUIEffect>("Resources/Prefab2/ScreenUIEffect/ScreenUIEffect", layerType);
			_gameUiDic.Add(layerType, windowPrefab);
		}
		windowPrefab.instance.OpenInternal(prefabName);
		return windowPrefab.instance;
	}

	public static void OnSceneChanged()
	{
		for (int i = 0; i < _gameUiDic.Count; i++)
		{
			WindowPrefab<ScreenUIEffect> valueByIndex = _gameUiDic.GetValueByIndex(i);
			valueByIndex.CloseWindow();
			valueByIndex.ReleaseWindow();
		}
	}

	internal void OpenInternal(string prefabName)
	{
		GameObject gameObject = Resources.Load(EffectPath + prefabName) as GameObject;
		if (gameObject == null)
		{
			VespaDebug.LogError("No Exist ScreenUIEffect - " + prefabName);
			LastEffect = null;
			Stop(null);
		}
		else if (!CreateUIEffect(gameObject))
		{
			VespaDebug.LogError("No Create ScreenUIEffect - " + prefabName);
			LastEffect = null;
			Stop(null);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private bool CreateUIEffect(GameObject prefab)
	{
		GameObject gameObject = Object.Instantiate(prefab, Root);
		if (gameObject == null)
		{
			return false;
		}
		LastEffect = gameObject;
		EffectList.Add(gameObject);
		return true;
	}

	private void RemoveUIEffect(GameObject effectObject)
	{
		if (!(effectObject == null))
		{
			EffectList.Remove(effectObject);
			VespaObject.DestroyGo(effectObject);
		}
	}

	private void RemoveAllUIEffect()
	{
		foreach (GameObject effect in EffectList)
		{
			VespaObject.DestroyGo(effect);
		}
		EffectList.Clear();
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
		RemoveAllUIEffect();
		LastEffect = null;
		base.gameObject.SetActive(value: false);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Stop(GameObject effect)
	{
		RemoveUIEffect(effect);
		if (__Extension_Method_ICollection__.COUNT(EffectList) == 0)
		{
			((IWindow)this).Close();
		}
	}

	public void AllStop()
	{
		((IWindow)this).Close();
	}

	private void OnDestroy()
	{
		RemoveAllUIEffect();
	}
}
