using System;
using System.Collections;
using NGame2.NScene.NPreloader;
using UnityEngine;

namespace NGame2.NUI.NWindow.NLoading;

public class LoadingBlack : MonoBehaviour, IGlobalWindow
{
	public interface ILoadingStatus
	{
		string Label { get; }

		float FillAmount { get; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Loading/LoadingBlack";

	private static GlobalWindowPrefab<LoadingBlack> _prefabInstance = new GlobalWindowPrefab<LoadingBlack>("Resources/Prefab2/Window/Loading/LoadingBlack", GlobalUIRoot.ELayerType.GlobalUI);

	public float FadeTime = 0.3f;

	public UIWidget RootWidget;

	public static LoadingBlack instance => _prefabInstance.instance;

	public static bool isValidInstance => _prefabInstance.IsValidInstance;

	public static void CreateInstance()
	{
		_prefabInstance.CreateInstance();
	}

	public static void ReleaseInstance()
	{
		_prefabInstance.ReleaseWindow();
	}

	public void OpenFadeIn(Action onComplete)
	{
		RootWidget.alpha = 0f;
		VespaCoroutine.Start(coFadeIn(onComplete));
	}

	private IEnumerator coFadeIn(Action onComplete)
	{
		base.gameObject.SetActive(value: true);
		yield return null;
		float beginTime = Time.realtimeSinceStartup;
		float num;
		do
		{
			yield return null;
			num = Time.realtimeSinceStartup - beginTime;
			float alpha = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(FadeTime));
			RootWidget.alpha = alpha;
		}
		while (!(num >= FadeTime));
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public void CloseFadeOut(Action onComplete, bool releaseSelf)
	{
		VespaCoroutine.Start(coCloseFadeOut(onComplete, releaseSelf));
	}

	private IEnumerator coCloseFadeOut(Action onComplete, bool releaseSelf)
	{
		yield return VespaCoroutine.Start(coFadeOut());
		Vespa_System_Action_Extension_Method.fire(onComplete);
		if (releaseSelf)
		{
			ReleaseInstance();
		}
	}

	private IEnumerator coFadeOut()
	{
		yield return null;
		float beginTime = Time.realtimeSinceStartup;
		float num;
		do
		{
			yield return null;
			num = Time.realtimeSinceStartup - beginTime;
			float num2 = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(FadeTime));
			RootWidget.alpha = 1f - num2;
		}
		while (!(num >= FadeTime));
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
