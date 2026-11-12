using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class BaseData : IValidation, IDrawInspector
{
	public enum EUpdateValueType
	{
		None,
		Time,
		ScrollViewHorizontal,
		ScrollViewVertical
	}

	public enum EAnimationDirection
	{
		ZeroToOne,
		OneToZero
	}

	public bool Use;

	public bool IsEnableChangeUpdateType = true;

	public EUpdateValueType UpdateValueType;

	public float Delay;

	public float Value;

	public bool UseCurve;

	public EAnimationDirection AnimationCurveDirection;

	public AnimationCurve AnimationCurve = new AnimationCurve();

	public BaseData()
	{
		UpdateValueType = EUpdateValueType.Time;
	}

	public BaseData(bool isEnableChangeUpdateType, EUpdateValueType updateValueType)
	{
		IsEnableChangeUpdateType = isEnableChangeUpdateType;
		UpdateValueType = updateValueType;
	}

	public bool IsValid()
	{
		if (UpdateValueType == EUpdateValueType.None)
		{
			return false;
		}
		if (Use)
		{
			return Value > 0f;
		}
		return false;
	}

	public virtual IUpdater CreateUpdater(GameObject[] objs, CreateUpdaterStock stock)
	{
		return null;
	}

	public virtual IUpdater CreateUpdater(GameObject[] objs, float delay, bool isReverse)
	{
		return null;
	}

	public virtual void DrawInspectorParts()
	{
	}

	public virtual void DrawInspector()
	{
	}
}
