using System;
using System.Collections;
using NShader;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ScreenFilterWindow : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/ScreenFilterWindow";

	public UITexture FilterTexture;

	private IFilter _filter;

	private bool _setFilterTexture;

	private Coroutine _coroutine;

	private Action _onComplete;

	public float OpenFadeTimeLength => 0.05f;

	public float CloseFadeTimeLength => 0.2f;

	public void Open(IFilter filter, Action onComplete)
	{
		base.Open();
		_filter = filter;
		SetEventHide(0f);
		StopCoroutineAndComplete();
		_onComplete = onComplete;
		_coroutine = VespaCoroutine.Start(this, coOpen());
	}

	private IEnumerator coOpen()
	{
		SetFilterTexture();
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += Time.deltaTime;
			float eventHide = NVespa.Math.CalcFrame(elapsedTime, OpenFadeTimeLength);
			SetEventHide(eventHide);
		}
		while (!(elapsedTime >= OpenFadeTimeLength));
		GlobalAutoBehaviour<GlobalCameraManager>.instance.PushMinCameraDepth(base.gameObject.layer, this);
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
		_coroutine = null;
	}

	public void Close(Action onComplete)
	{
		StopCoroutineAndComplete();
		if (!base.gameObject.activeInHierarchy)
		{
			GlobalAutoBehaviour<GlobalCameraManager>.instance.PopMinCameraDepth(this);
			ReleaseFilterTexture();
			Vespa_System_Action_Extension_Method.fire(onComplete);
			base.Close();
		}
		else
		{
			_onComplete = delegate
			{
				Vespa_System_Action_Extension_Method.fire(onComplete);
				base.Close();
			};
			_coroutine = VespaCoroutine.Start(this, coClose());
		}
	}

	private IEnumerator coClose()
	{
		GlobalAutoBehaviour<GlobalCameraManager>.instance.PopMinCameraDepth(this);
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += Time.deltaTime;
			float eventHide = 1f - NVespa.Math.CalcFrame(elapsedTime, CloseFadeTimeLength);
			SetEventHide(eventHide);
		}
		while (!(elapsedTime >= CloseFadeTimeLength));
		ReleaseFilterTexture();
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
		_coroutine = null;
	}

	public override void Reset()
	{
		base.Reset();
	}

	private void SetFilterTexture()
	{
		if (!_setFilterTexture && _filter != null && GlobalAutoBehaviour<GlobalCameraManager>.instance.TryGetCameraDepthByLayer(base.gameObject.layer, out var cameraDepth))
		{
			RenderTexture renderTexture = GlobalAutoBehaviour<GlobalCameraManager>.instance.CaptureCamera(Mathf.RoundToInt(cameraDepth), _filter);
			if (!(renderTexture == null))
			{
				FilterTexture.gameObject.SetActive(value: true);
				FilterTexture.mainTexture = renderTexture;
				FilterTexture.alpha = 1f;
				_setFilterTexture = true;
			}
		}
	}

	private void ReleaseFilterTexture()
	{
		if (_setFilterTexture)
		{
			_setFilterTexture = false;
			RenderTexture renderTexture = FilterTexture.mainTexture as RenderTexture;
			if (renderTexture != null)
			{
				VespaObject.DestroyObject(renderTexture);
				FilterTexture.mainTexture = null;
				FilterTexture.gameObject.SetActive(value: false);
			}
		}
	}

	public void Show(bool show)
	{
		if (_setFilterTexture)
		{
			if (!show)
			{
				GlobalAutoBehaviour<GlobalCameraManager>.instance.PopMinCameraDepth(this);
			}
			else
			{
				GlobalAutoBehaviour<GlobalCameraManager>.instance.PushMinCameraDepth(base.gameObject.layer, this);
			}
		}
	}

	private void StopCoroutineAndComplete()
	{
		if (_coroutine != null)
		{
			VespaCoroutine.Stop(this, _coroutine);
			_coroutine = null;
		}
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
	}
}
