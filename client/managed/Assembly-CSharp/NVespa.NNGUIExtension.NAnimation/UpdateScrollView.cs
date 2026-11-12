using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class UpdateScrollView : BaseUpdateValue
{
	protected float _minRange;

	protected float _maxRange;

	protected float _centerPos;

	protected float _scrollPos;

	protected float _inverseRange;

	public void Setup(float pos, float range, float scrollPos)
	{
		_centerPos = pos;
		_scrollPos = scrollPos;
		float num = range * 0.5f;
		_minRange = _centerPos - num;
		_maxRange = _centerPos + num;
		if (range != 0f)
		{
			_inverseRange = 1f / num;
		}
	}

	public override bool CanUpdate(float value)
	{
		UpdateValue(value);
		return true;
	}

	protected override void UpdateValue(float value)
	{
		_scrollPos = value;
	}

	public override bool IsWaiting(float value)
	{
		return false;
	}

	public override bool IsEnd()
	{
		return false;
	}

	public override float GetRatio()
	{
		if (_minRange <= _scrollPos && _scrollPos <= _maxRange)
		{
			float f = _centerPos - _scrollPos;
			return Mathf.Clamp01(1f - Mathf.Abs(f) * _inverseRange);
		}
		return 0f;
	}

	public override void Reset()
	{
		_minRange = 0f;
		_maxRange = 0f;
		_centerPos = 0f;
		_inverseRange = 0f;
		_scrollPos = 0f;
	}

	public override void DrawInspector()
	{
	}
}
