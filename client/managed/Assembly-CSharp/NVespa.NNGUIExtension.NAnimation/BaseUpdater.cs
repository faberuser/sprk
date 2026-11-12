using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class BaseUpdater<T> : IValidation, IUpdater, IDrawInspector, IDrawGizmos where T : BaseData
{
	protected GameObject[] _objs;

	protected T _data;

	protected float _delayTime;

	protected bool _isReverse;

	protected BaseUpdateValue _updateValue;

	public BaseData.EUpdateValueType UpdateValueType
	{
		get
		{
			if (_data == null)
			{
				return BaseData.EUpdateValueType.None;
			}
			return _data.UpdateValueType;
		}
	}

	public virtual bool Setup(T data, GameObject[] objs, float value, bool isReverse)
	{
		if (data == null || !data.IsValid() || __Extension_Method_ICollection__.IsNullOrEmpty(objs))
		{
			return false;
		}
		_objs = objs;
		SetData(data, isReverse);
		SetUpdateValueType(objs[0], value);
		return true;
	}

	public bool Setup(T data, GameObject obj, float value, bool isReverse)
	{
		return Setup(data, new GameObject[1] { obj }, value, isReverse);
	}

	private void SetData(T data, bool isReverse)
	{
		_data = data;
		_isReverse = isReverse;
	}

	private void SetUpdateValueType(GameObject obj, float value)
	{
		if (_data.UpdateValueType == BaseData.EUpdateValueType.None)
		{
			return;
		}
		if (_data.UpdateValueType == BaseData.EUpdateValueType.Time)
		{
			_delayTime = value;
			UpdateTimeValue updateTimeValue = new UpdateTimeValue();
			updateTimeValue.Setup(_data.Value, _data.Delay, _isReverse);
			_updateValue = updateTimeValue;
		}
		else if (_data.UpdateValueType == BaseData.EUpdateValueType.ScrollViewHorizontal)
		{
			if (obj == null)
			{
				_updateValue = null;
				return;
			}
			UpdateScrollView updateScrollView = new UpdateScrollView();
			updateScrollView.Setup(obj.transform.localPosition.x, _data.Value, value);
			_updateValue = updateScrollView;
		}
		else if (_data.UpdateValueType == BaseData.EUpdateValueType.ScrollViewVertical)
		{
			if (obj == null)
			{
				_updateValue = null;
				return;
			}
			UpdateScrollView updateScrollView2 = new UpdateScrollView();
			updateScrollView2.Setup(obj.transform.localPosition.y, _data.Value, value);
			_updateValue = updateScrollView2;
		}
	}

	public virtual void SetEnd()
	{
	}

	public virtual void OnUpdate(float delta)
	{
	}

	public bool IsValid()
	{
		if (_data == null || !_data.IsValid())
		{
			return false;
		}
		if (_updateValue == null)
		{
			return false;
		}
		return true;
	}

	protected bool CanUpdateAnimation(float value)
	{
		if (!IsValid())
		{
			return false;
		}
		if (!_isReverse && IsWaiting(value))
		{
			return false;
		}
		if (!_updateValue.CanUpdate(value))
		{
			return false;
		}
		return true;
	}

	protected bool IsWaiting(float delta)
	{
		if (!_data.IsValid())
		{
			return true;
		}
		if (_data.UpdateValueType == BaseData.EUpdateValueType.Time)
		{
			if (_delayTime > 0f)
			{
				_delayTime -= delta;
			}
			return _delayTime > 0f;
		}
		return false;
	}

	public virtual void Refresh()
	{
	}

	public virtual bool IsEnd()
	{
		if (!IsValid())
		{
			return true;
		}
		return _updateValue.IsEnd();
	}

	public float GetRatio()
	{
		if (!IsValid())
		{
			return 0f;
		}
		if (_isReverse)
		{
			return 1f - _updateValue.GetRatio();
		}
		return _updateValue.GetRatio();
	}

	public float GetAnimationCurveWeight(float ratio)
	{
		if (!IsValid())
		{
			return ratio;
		}
		if (_data.AnimationCurveDirection == BaseData.EAnimationDirection.OneToZero)
		{
			ratio = 1f - ratio;
		}
		if (_data.UseCurve)
		{
			return Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_data.AnimationCurve, ratio, ratio);
		}
		return ratio;
	}

	public void Reset()
	{
		if (IsValid())
		{
			Release();
		}
	}

	public virtual void Release()
	{
		_objs = null;
		_data = null;
		_delayTime = 0f;
		_isReverse = false;
		if (_updateValue != null)
		{
			_updateValue.Reset();
			_updateValue = null;
		}
	}

	public virtual void DrawInspector()
	{
	}

	public virtual void DrawInspectorParts()
	{
	}

	public virtual void OnDrawGizmos()
	{
	}
}
