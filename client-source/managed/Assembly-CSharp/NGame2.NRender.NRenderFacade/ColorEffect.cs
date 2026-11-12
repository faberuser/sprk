using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NRender.NRenderFacade;

public class ColorEffect
{
	public interface IValue
	{
		bool IsEnd();

		Color GetColor();
	}

	private class ConstantValue : IValue
	{
		internal Color _color;

		internal float _beginTime;

		internal float _timeLength;

		public bool IsEnd()
		{
			return _beginTime + _timeLength <= GlobalTimer.time;
		}

		public Color GetColor()
		{
			return _color;
		}
	}

	private class GradientValue : IValue
	{
		internal Gradient _gradient;

		internal float _beginTime;

		internal float _timeLength;

		public bool IsEnd()
		{
			return _beginTime + _timeLength <= GlobalTimer.time;
		}

		public Color GetColor()
		{
			float to = _beginTime + _timeLength;
			float i_frame = Math.InverseLerpInc(_beginTime, to, GlobalTimer.time);
			return Vespa_Unity_Gradient_Extension_Method.EVALUATE(_gradient, i_frame, Colors.zero);
		}
	}

	private string _damageColorName;

	private RenderFacade _facade;

	private List<IValue> _listItem = new List<IValue>();

	public ColorEffect(RenderFacade facade)
	{
		_facade = facade;
	}

	public void SetDamageColorName(string name)
	{
		_damageColorName = name;
	}

	public void Damage()
	{
		Gradient gradient = GameSetting.data.DamageColor.GetGradient(_damageColorName);
		SetGradient(gradient, GameSetting.data.DamageColor.TimeLength);
	}

	public void SetGradient(Gradient gradient, float timeLength)
	{
		if (!(timeLength < Mathf.Epsilon))
		{
			GradientValue gradientValue = new GradientValue();
			gradientValue._gradient = gradient;
			gradientValue._beginTime = GlobalTimer.time;
			gradientValue._timeLength = timeLength;
			_listItem.Add(gradientValue);
		}
	}

	public void SetConstantColor(Color color, float timeLength)
	{
		if (!(timeLength < Mathf.Epsilon))
		{
			ConstantValue constantValue = new ConstantValue();
			constantValue._color = color;
			constantValue._beginTime = GlobalTimer.time;
			constantValue._timeLength = timeLength;
			_listItem.Add(constantValue);
		}
	}

	public void AddValue(IValue value)
	{
		_listItem.Add(value);
	}

	public void ChainUpdate()
	{
		Vector4 color = CalcColorMix();
		_facade.RenderGroupModifier?.ColorMix(color);
	}

	private Vector4 CalcColorMix()
	{
		Vector4 zero = Vector4.zero;
		for (int i = 0; i < _listItem.Count; i++)
		{
			IValue value = _listItem[i];
			if (value.IsEnd())
			{
				_listItem.RemoveAt(i);
				i--;
				continue;
			}
			Color color = value.GetColor();
			zero.x = Mathf.Clamp01(zero.x + color.r);
			zero.y = Mathf.Clamp01(zero.y + color.g);
			zero.z = Mathf.Clamp01(zero.z + color.b);
			zero.w = Mathf.Clamp01(zero.w + color.a);
		}
		return zero;
	}
}
