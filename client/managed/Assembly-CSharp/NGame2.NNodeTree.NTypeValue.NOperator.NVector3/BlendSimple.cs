using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NVector3;

public class BlendSimple : ITypeValue<Vector3>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public BlendSimple()
	{
	}

	public BlendSimple(BlendSimple prototype)
	{
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
		o_value = NVespa.Math.LerpOver(value, value2, value3);
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
		return new BlendSimple(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		return jsonData;
	}
}
