using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class ShakeEffect
{
	public interface IValue
	{
		bool IsEnd();

		Vector3 GetForce();
	}

	private class DefeatShake
	{
		public float BeginTime;

		public Vector3 Direction;

		public bool IsEnd()
		{
			return GlobalTimer.time - BeginTime > GameSetting.data.DamageShake.TimeLength;
		}

		public Vector3 GetForce()
		{
			float num = GlobalTimer.time - BeginTime;
			if (num > GameSetting.data.DamageShake.TimeLength)
			{
				return Vector3.zero;
			}
			float timeLength = GameSetting.data.DamageShake.TimeLength;
			float num2 = timeLength * GameSetting.data.DamageShake.MidFrame;
			if (num < num2)
			{
				float f = num / Vespa_Float_Extension_Method.NoneZero(num2);
				f = Mathf.Pow(f, 2f);
				return Direction * f * GameSetting.data.DamageShake.Distance;
			}
			float f2 = Mathf.InverseLerp(num2, timeLength, num);
			f2 = Mathf.Pow(f2, 2f);
			return Direction * (1f - f2) * GameSetting.data.DamageShake.Distance;
		}
	}

	private List<IValue> _valueList = new List<IValue>();

	private List<DefeatShake> _defeatList = new List<DefeatShake>();

	private VespaList<Vector3> _calculateList = new VespaList<Vector3>(32);

	public Vector3 FinalValue { get; private set; }

	public ShakeEffect(RenderFacade facade)
	{
	}

	public void Damage(VectorXZ direction)
	{
		DefeatShake defeatShake = new DefeatShake();
		defeatShake.BeginTime = GlobalTimer.time;
		defeatShake.Direction = direction.ToVec3();
		_defeatList.Add(defeatShake);
	}

	public void AddValue(IValue value)
	{
		_valueList.Add(value);
	}

	public void ChainUpdate()
	{
		CollectDefeatShake();
		CollectValueShake();
		FinalValue = Vector3.zero;
		if (_calculateList.Count != 0)
		{
			_calculateList.Sort((Vector3 lhs, Vector3 rhs) => rhs.sqrMagnitude.CompareTo(lhs.sqrMagnitude));
			for (int num = 0; num < _calculateList.Count; num++)
			{
				FinalValue += _calculateList[num];
			}
			float magnitude = _calculateList[0].magnitude;
			float magnitude2 = FinalValue.magnitude;
			if (magnitude2 > magnitude && magnitude > Mathf.Epsilon)
			{
				float num2 = 2f - magnitude / magnitude2;
				FinalValue = FinalValue.normalized * magnitude * num2;
			}
			_calculateList.Clear();
		}
	}

	private void CollectDefeatShake()
	{
		for (int i = 0; i < _defeatList.Count; i++)
		{
			DefeatShake defeatShake = _defeatList[i];
			if (defeatShake.IsEnd())
			{
				_defeatList.RemoveAt(i);
				i--;
			}
			else
			{
				_calculateList.Add(defeatShake.GetForce());
			}
		}
	}

	private void CollectValueShake()
	{
		for (int i = 0; i < _valueList.Count; i++)
		{
			IValue value = _valueList[i];
			if (value.IsEnd())
			{
				_valueList.RemoveAt(i);
				i--;
			}
			else
			{
				_calculateList.Add(value.GetForce());
			}
		}
	}
}
