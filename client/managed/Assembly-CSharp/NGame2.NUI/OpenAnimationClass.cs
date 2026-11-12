using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGame2.NUI;

public class OpenAnimationClass : MonoBehaviour
{
	public enum AnimationType
	{
		Alpha,
		Scale,
		PositionX,
		PositionY
	}

	[Serializable]
	public class AnimationInfo
	{
		public AnimationType Type;

		public AnimationCurve Curve;

		public float Start;

		public float End;

		public float Lerp(float t)
		{
			return Mathf.Lerp(Start, End, Curve.Evaluate(t));
		}
	}

	[SerializeField]
	private float PlayTime;

	[SerializeField]
	private float DelayTime;

	[SerializeField]
	private List<AnimationInfo> AnimationInfos = new List<AnimationInfo>();

	private UIWidget _widget;

	private UIPanel _panel;

	private bool _isPlay;

	private float _time;

	private float _delayTime;

	private bool _isForward = true;

	private void OnDisable()
	{
		Finish();
	}

	public void Ready(bool isForward = true)
	{
		bool isForward2 = _isForward;
		_isForward = isForward;
		UpdateAll(0f);
		_isForward = isForward2;
	}

	public void Play(float addedDelayTime = 0f, bool isForward = true)
	{
		_isPlay = true;
		_time = 0f;
		_delayTime = DelayTime + addedDelayTime;
		_isForward = isForward;
		UpdateAll(0f);
	}

	public void Finish()
	{
		UpdateAll(1f);
		_isForward = true;
		_isPlay = false;
	}

	private void Update()
	{
		if (!_isPlay)
		{
			return;
		}
		_time += Time.deltaTime;
		if (!(_time <= _delayTime))
		{
			float num = _time - _delayTime;
			if (num < PlayTime)
			{
				UpdateAll(num / PlayTime);
			}
			else
			{
				Finish();
			}
		}
	}

	private void UpdateAll(float t)
	{
		if (!_isForward)
		{
			t = 1f - t;
		}
		foreach (AnimationInfo animationInfo in AnimationInfos)
		{
			switch (animationInfo.Type)
			{
			case AnimationType.Alpha:
				UpdateAlpha(animationInfo, t);
				break;
			case AnimationType.Scale:
				UpdateScale(animationInfo, t);
				break;
			case AnimationType.PositionX:
				UpdatePositionX(animationInfo, t);
				break;
			case AnimationType.PositionY:
				UpdatePositionY(animationInfo, t);
				break;
			}
		}
	}

	private void UpdateAlpha(AnimationInfo info, float t)
	{
		if (_widget == null && _panel == null)
		{
			_widget = GetComponent<UIWidget>();
			_panel = GetComponent<UIPanel>();
		}
		if (_panel != null)
		{
			_panel.alpha = info.Lerp(t);
		}
		else if (_widget != null)
		{
			_widget.alpha = info.Lerp(t);
		}
	}

	private void UpdateScale(AnimationInfo info, float t)
	{
		base.transform.localScale = Vector3.one * info.Lerp(t);
	}

	private void UpdatePositionX(AnimationInfo info, float t)
	{
		Vector3 localPosition = base.transform.localPosition;
		base.transform.localPosition = new Vector3(info.Lerp(t), localPosition.y, localPosition.z);
	}

	private void UpdatePositionY(AnimationInfo info, float t)
	{
		Vector3 localPosition = base.transform.localPosition;
		base.transform.localPosition = new Vector3(localPosition.x, info.Lerp(t), localPosition.z);
	}
}
