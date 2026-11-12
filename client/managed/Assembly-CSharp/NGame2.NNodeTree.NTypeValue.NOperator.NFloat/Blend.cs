using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NFloat;

public class Blend : ITypeValue<float>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private AnimationCurve _curve = new AnimationCurve();

	string ITypeValueBase.ValuePath { get; set; }

	public Blend()
	{
	}

	public Blend(Blend prototype)
	{
		_curve = prototype._curve;
	}

	float ITypeValue<float>.GetValue(IVariable variable)
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((ITypeValue<float>)this).TryGetValue(variable, out var o_value))
		{
			return 0f;
		}
		return o_value;
	}

	bool ITypeValue<float>.TryGetValue(IVariable variable, out float o_value)
	{
		ITypeValue<float> typeValue = _argumentContainer.GetTypeValue<float>(0);
		ITypeValue<float> typeValue2 = _argumentContainer.GetTypeValue<float>(1);
		ITypeValue<float> typeValue3 = _argumentContainer.GetTypeValue<float>(2);
		if (typeValue == null || typeValue2 == null || typeValue3 == null)
		{
			o_value = 0f;
			return true;
		}
		float value = typeValue.GetValue(variable);
		float value2 = typeValue2.GetValue(variable);
		float value3 = typeValue3.GetValue(variable);
		o_value = NVespa.Math.LerpOver(value, value2, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_curve, value3, value3));
		return true;
	}

	Type ITypeValueBase.GetValueType()
	{
		return typeof(float);
	}

	object ITypeValueBase.GetObjectValue(IVariable variable)
	{
		if (!this.TRYGETVALUE(variable, out var o_value))
		{
			return null;
		}
		return o_value;
	}

	ArgumentContainer ITypeValueBase.GetArgumentContainer()
	{
		return _argumentContainer;
	}

	ITypeValueBase ITypeValueBase.CreateFromPrototype()
	{
		return new Blend(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		_curve = JsonTools.ToCurve(json["Curve"], _curve);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		jsonData.AddField("Curve", JsonTools.FromCurve(_curve));
		return jsonData;
	}
}
