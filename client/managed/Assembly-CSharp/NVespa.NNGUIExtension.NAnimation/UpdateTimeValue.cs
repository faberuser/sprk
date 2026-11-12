using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class UpdateTimeValue : BaseUpdateValue
{
	protected float _duration;

	protected float _playedTime;

	protected float _delayTime;

	protected float _cacehdDelayTime;

	protected float _inverseDuration;

	protected bool _isReverse;

	public void Setup(float duration, float delayTime, bool isReverse)
	{
		_duration = duration;
		_delayTime = delayTime;
		_cacehdDelayTime = delayTime;
		_isReverse = isReverse;
		if (_duration != 0f)
		{
			_inverseDuration = 1f / _duration;
		}
	}

	public override bool CanUpdate(float value)
	{
		if (IsEnd())
		{
			return false;
		}
		if (IsWaiting(value))
		{
			return false;
		}
		UpdateValue(value);
		return true;
	}

	protected override void UpdateValue(float value)
	{
		_playedTime += value;
	}

	public override bool IsEnd()
	{
		return _duration < _playedTime;
	}

	public override bool IsWaiting(float value)
	{
		if (_isReverse)
		{
			return false;
		}
		if (_delayTime > 0f)
		{
			_delayTime -= value;
		}
		return _delayTime > 0f;
	}

	public override void Reset()
	{
		_duration = 0f;
		_playedTime = 0f;
		_delayTime = 0f;
		_cacehdDelayTime = 0f;
		_inverseDuration = 0f;
		_isReverse = false;
	}

	public override float GetRatio()
	{
		return Mathf.Clamp01(_playedTime * _inverseDuration);
	}

	public override void DrawInspector()
	{
	}

	public override void Replay()
	{
		_delayTime = _cacehdDelayTime;
		_playedTime = 0f;
	}
}
