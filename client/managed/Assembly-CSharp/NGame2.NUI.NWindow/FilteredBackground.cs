using System;
using System.Collections;
using NShader;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class FilteredBackground : BaseWindow
{
	public Transform Root;

	public UIPanel BackgroundPanel;

	public UITexture BlurTexture;

	public UITexture BlackTexture;

	public AnimationCurve FadeCurve = new AnimationCurve();

	public float FadeTimeLength_Black = 0.5f;

	public float FadeTimeLength_Filter = 1f;

	protected bool _backgroundFilter;

	protected Coroutine _coroutine;

	protected Action _onComplete;

	public float FadeTimeLength
	{
		get
		{
			if (!_backgroundFilter)
			{
				return FadeTimeLength_Black;
			}
			return FadeTimeLength_Filter;
		}
	}

	public new void Open()
	{
		base.Open();
	}

	public void OpenFilter(IFilter filter, bool skipAnim, Action onComplete)
	{
		UIHelper.EnableGameObject(BackgroundPanel, enable: true);
		BackgroundPanel.alpha = 0f;
		StopCoroutineAndComplete();
		_onComplete = onComplete;
		_coroutine = VespaCoroutine.Start(this, coOpen(filter, skipAnim));
	}

	public void SetColor(Color color)
	{
		UIHelper.SetWidgetColor(BlurTexture, color);
		UIHelper.SetWidgetColor(BlackTexture, color);
	}

	private IEnumerator coOpen(IFilter filter, bool skipAnim)
	{
		if (filter != null)
		{
			yield return null;
			UIHelper.EnableGameObject(BlurTexture, enable: true);
			UIHelper.EnableGameObject(BlackTexture, enable: false);
			SetBackgroundFilter(filter);
		}
		else
		{
			UIHelper.EnableGameObject(BlurTexture, enable: false);
			UIHelper.EnableGameObject(BlackTexture, enable: true);
		}
		if (!skipAnim)
		{
			float elapsedTime = 0f;
			do
			{
				yield return null;
				elapsedTime += Time.deltaTime;
				float num = NVespa.Math.CalcFrame(elapsedTime, FadeTimeLength);
				float curveFrame = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(FadeCurve, num, num);
				UpdateFrame(curveFrame);
			}
			while (!(elapsedTime >= FadeTimeLength));
		}
		else
		{
			float curveFrame2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(FadeCurve, 1f, 1f);
			UpdateFrame(curveFrame2);
		}
		if (filter != null)
		{
			GlobalAutoBehaviour<GlobalCameraManager>.instance.PushMinCameraDepth(base.gameObject.layer, this);
		}
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
		_coroutine = null;
	}

	protected virtual void UpdateFrame(float curveFrame)
	{
		BackgroundPanel.alpha = curveFrame;
	}

	public void Close(bool isHide, bool skipAnimation, Action onComplete)
	{
		StopCoroutineAndComplete();
		if (!base.gameObject.activeInHierarchy)
		{
			if (_backgroundFilter)
			{
				GlobalAutoBehaviour<GlobalCameraManager>.instance.PopMinCameraDepth(this);
				ReleaseBackgroundFilter();
			}
			Vespa_System_Action_Extension_Method.fire(onComplete);
			if (isHide)
			{
				UIHelper.EnableGameObject(BackgroundPanel, enable: false);
			}
			else
			{
				base.Close();
			}
			return;
		}
		_onComplete = delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			if (isHide)
			{
				UIHelper.EnableGameObject(BackgroundPanel, enable: false);
			}
			else
			{
				base.Close();
			}
		};
		_coroutine = VespaCoroutine.Start(this, coClose(skipAnimation));
	}

	private IEnumerator coClose(bool skipAnimation)
	{
		if (_backgroundFilter)
		{
			GlobalAutoBehaviour<GlobalCameraManager>.instance.PopMinCameraDepth(this);
		}
		if (!skipAnimation)
		{
			float elapsedTime = 0f;
			do
			{
				yield return null;
				elapsedTime += Time.deltaTime;
				float num = 1f - NVespa.Math.CalcFrame(elapsedTime, FadeTimeLength);
				float curveFrame = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(FadeCurve, num, num);
				UpdateFrame(curveFrame);
			}
			while (!(elapsedTime >= FadeTimeLength));
		}
		else
		{
			UpdateFrame(0f);
		}
		ReleaseBackgroundFilter();
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
		_coroutine = null;
	}

	private void SetBackgroundFilter(IFilter filter)
	{
		if (!_backgroundFilter && GlobalAutoBehaviour<GlobalCameraManager>.instance.TryGetCameraDepthByLayer(base.gameObject.layer, out var cameraDepth))
		{
			RenderTexture renderTexture = GlobalAutoBehaviour<GlobalCameraManager>.instance.CaptureCamera(Mathf.RoundToInt(cameraDepth), filter);
			if (!(renderTexture == null))
			{
				BlurTexture.gameObject.SetActive(value: true);
				BlurTexture.mainTexture = renderTexture;
				_backgroundFilter = true;
			}
		}
	}

	private void ReleaseBackgroundFilter()
	{
		if (!_backgroundFilter)
		{
			UIHelper.EnableGameObject(BlackTexture, enable: false);
			UIHelper.EnableGameObject(BlurTexture, enable: false);
			return;
		}
		_backgroundFilter = false;
		RenderTexture renderTexture = BlurTexture.mainTexture as RenderTexture;
		if (renderTexture != null)
		{
			VespaObject.DestroyObject(renderTexture);
			BlurTexture.mainTexture = null;
			BlurTexture.gameObject.SetActive(value: false);
		}
	}

	public void Show(bool show)
	{
		if (_backgroundFilter)
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
		UIHelper.EnableGameObject(Root, show);
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
