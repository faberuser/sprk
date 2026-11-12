using System.Collections.Generic;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NGame3DWidget;
using NGameAsset;
using NGameAsset.NEffect;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class WaitingPopup : MonoBehaviour, IGlobalWindow
{
	public enum ImageType
	{
		KaselRun,
		MarkRotate
	}

	public const string PrefabPath = "Resources/Prefab2/Window/WaitingPopup";

	private static GlobalWindowPrefab<WaitingPopup> _windowPrefab = new GlobalWindowPrefab<WaitingPopup>("Resources/Prefab2/Window/WaitingPopup", GlobalUIRoot.ELayerType.GlobalUI);

	public UIWidget WaitingMarkWidget;

	public float RotateTime = 1f;

	public UIGame3DWidget WaitingEffect;

	private IContents _effectContents;

	private List<object> _waiterList = new List<object>();

	private const string EffectCode = "UI.Loading.KaselRun";

	private static OpenData _preloadEffect;

	public static bool BlockWaitingPopup { get; set; }

	public static void PreloadWaitingAsset()
	{
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get("UI.Loading.KaselRun");
		if (builder != null)
		{
			_preloadEffect = builder.CreateOpenData();
		}
	}

	public static void ReleasePreload()
	{
		if (_preloadEffect != null)
		{
			_preloadEffect.Close();
			_preloadEffect = null;
		}
	}

	public static void Open(object waiter, ImageType type = ImageType.KaselRun)
	{
		_windowPrefab.instance.SetImageType(type);
		_windowPrefab.instance.OpenInternal(waiter, GlobalUIRoot.ELayerType.GlobalUI);
	}

	public static void OpenWithLayer(object waiter, GlobalUIRoot.ELayerType layerType, ImageType type = ImageType.KaselRun)
	{
		_windowPrefab.instance.SetImageType(type);
		_windowPrefab.instance.OpenInternal(waiter, layerType);
	}

	public static void Close(object waiter)
	{
		_windowPrefab.instance.CloseInternal(waiter);
	}

	public static bool Contains(object waiter)
	{
		return _windowPrefab.instance._waiterList.Contains(waiter);
	}

	public static bool IsActivePopup()
	{
		return _windowPrefab.instance.IsActive();
	}

	internal bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	internal void SetImageType(ImageType type)
	{
		UIHelper.EnableGameObject(WaitingMarkWidget, type == ImageType.MarkRotate);
		UIHelper.EnableGameObject(WaitingEffect, type == ImageType.KaselRun);
	}

	internal void OpenInternal(object waiter, GlobalUIRoot.ELayerType layerType)
	{
		if (BlockWaitingPopup || waiter == null || _waiterList.Contains(waiter))
		{
			return;
		}
		_waiterList.Add(waiter);
		base.gameObject.SetActive(value: true);
		if (SingletonAuto<BundleManager>.isValidInstance && SingletonAuto<BundleManager>.instance.IsReady)
		{
			if (_effectContents != null)
			{
				_effectContents.ReleaseAll();
				_effectContents = null;
			}
			ScreenEffect.StockData stockData = new ScreenEffect.StockData();
			stockData.Code = "UI.Loading.KaselRun";
			stockData.IsBottom = true;
			_effectContents = ScreenEffect.PlayEffect(layerType, stockData);
		}
	}

	internal void CloseInternal(object waiter)
	{
		if (waiter != null && _waiterList.Remove(waiter) && _waiterList.Count == 0)
		{
			if (_effectContents != null && SingletonAuto<BundleManager>.instance.IsReady)
			{
				_effectContents.ReleaseAll();
				_effectContents = null;
			}
			Vespa_Common_Class_Extension_Method.Interface((IGlobalWindow)this).Close();
		}
	}

	void IGlobalWindow.ReserveCache(UiPreloader preloader)
	{
	}

	bool IGlobalWindow.IsOpen()
	{
		return base.gameObject.activeSelf;
	}

	void IGlobalWindow.Close()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		for (int i = 0; i < _waiterList.Count; i++)
		{
			if (_waiterList[i] == null)
			{
				_waiterList.RemoveAt(i);
				i--;
			}
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_waiterList))
		{
			Vespa_Common_Class_Extension_Method.Interface((IGlobalWindow)this).Close();
		}
		else
		{
			UpdateMark();
		}
	}

	private void UpdateMark()
	{
		if (!(WaitingMarkWidget == null) && WaitingMarkWidget.enabled)
		{
			float z = (float)(int)(Mathf.Repeat(Time.realtimeSinceStartup, RotateTime) / RotateTime * 12f) * 30f;
			WaitingMarkWidget.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, z));
		}
	}
}
