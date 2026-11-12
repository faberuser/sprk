using System.Collections;
using NVespa;
using UnityEngine;

public class UIButtonClickScale : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnDragOutNoArgListener
{
	public float PressTime = 0.05f;

	public float ReleaseTime = 0.05f;

	public float PressScale = 0.9f;

	public UIButtonClickScale Object_Linked;

	private bool _released;

	private Vector3 _origScale;

	private PoolingList<Coroutine> _coroutineList;

	private void OnDestroy()
	{
		Reset();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Reset()
	{
		if (_coroutineList.Count > 0)
		{
			for (int i = 0; i < _coroutineList.Count; i++)
			{
				Coroutine coroutine = _coroutineList[i];
				VespaCoroutine.Stop(this, coroutine);
			}
			_coroutineList.Clear();
			base.transform.localScale = _origScale;
			_released = false;
		}
	}

	public void OnDragOut()
	{
		_released = true;
	}

	public void OnPress(bool pressed)
	{
		if (pressed)
		{
			Reset();
			_origScale = base.transform.localScale;
			Coroutine item = VespaCoroutine.Start(this, coAnimate());
			_coroutineList.Add(item);
		}
		else
		{
			_released = true;
		}
		if (Object_Linked != null)
		{
			Object_Linked.OnPress(pressed);
		}
	}

	private IEnumerator coAnimate()
	{
		Coroutine coroutine = VespaCoroutine.Start(this, coAnimate(_origScale, PressTime, 1f, PressScale));
		_coroutineList.Add(coroutine);
		yield return coroutine;
		while (!_released)
		{
			yield return null;
		}
		Coroutine coroutine2 = VespaCoroutine.Start(this, coAnimate(_origScale, ReleaseTime, PressScale, 1f));
		_coroutineList.Add(coroutine2);
		yield return coroutine2;
	}

	private IEnumerator coAnimate(Vector3 origScale, float timeLength, float from, float to)
	{
		float beginTime = Time.realtimeSinceStartup;
		float num;
		do
		{
			yield return null;
			num = Time.realtimeSinceStartup - beginTime;
			float time = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(timeLength));
			float t = ConstantValue.EaseInOut0_1.Evaluate(time);
			float b = Mathf.Lerp(from, to, t);
			base.transform.localScale = origScale * Mathf.Max(0.1f, b);
		}
		while (!(num >= timeLength));
	}
}
