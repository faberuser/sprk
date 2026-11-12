using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NVector3;

public class BlendDirect : ITypeValue<Vector3>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private AnimationCurve _xCurve = new AnimationCurve();

	private AnimationCurve _yCurve = new AnimationCurve();

	private AnimationCurve _zCurve = new AnimationCurve();

	string ITypeValueBase.ValuePath { get; set; }

	public BlendDirect()
	{
	}

	public BlendDirect(BlendDirect prototype)
	{
		_xCurve = prototype._xCurve;
		_yCurve = prototype._yCurve;
		_zCurve = prototype._zCurve;
	}

	Vector3 ITypeValue<Vector3>.GetValue(IVariable variable)
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((ITypeValue<Vector3>)this).TryGetValue(variable, out var o_value))
		{
			return Vector3.zero;
		}
		return o_value;
	}

	bool ITypeValue<Vector3>.TryGetValue(IVariable variable, out Vector3 o_value)
	{
		ITypeValue<Vector3> typeValue = _argumentContainer.GetTypeValue<Vector3>(0);
		ITypeValue<Vector3> typeValue2 = _argumentContainer.GetTypeValue<Vector3>(1);
		ITypeValue<float> typeValue3 = _argumentContainer.GetTypeValue<float>(2);
		if (typeValue == null || typeValue2 == null || typeValue3 == null)
		{
			o_value = Vector3.zero;
			return true;
		}
		Vector3 value = typeValue.GetValue(variable);
		Vector3 value2 = typeValue2.GetValue(variable);
		float value3 = typeValue3.GetValue(variable);
		o_value.x = NVespa.Math.LerpOver(value.x, value2.x, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_xCurve, value3, value3));
		o_value.y = NVespa.Math.LerpOver(value.y, value2.y, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_yCurve, value3, value3));
		o_value.z = NVespa.Math.LerpOver(value.z, value2.z, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_zCurve, value3, value3));
		return true;
	}

	Type ITypeValueBase.GetValueType()
	{
		return typeof(Vector3);
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
		return new BlendDirect(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		_xCurve = JsonTools.ToCurve(json["xCurve"], _xCurve);
		_yCurve = JsonTools.ToCurve(json["yCurve"], _yCurve);
		_zCurve = JsonTools.ToCurve(json["zCurve"], _zCurve);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		jsonData.AddField("xCurve", JsonTools.FromCurve(_xCurve));
		jsonData.AddField("yCurve", JsonTools.FromCurve(_yCurve));
		jsonData.AddField("zCurve", JsonTools.FromCurve(_zCurve));
		return jsonData;
	}
}
