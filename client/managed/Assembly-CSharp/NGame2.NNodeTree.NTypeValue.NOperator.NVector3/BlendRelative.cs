using System;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NVector3;

public class BlendRelative : ITypeValue<Vector3>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	private AnimationCurve _forwardCurve = new AnimationCurve();

	private AnimationCurve _rightCurve = new AnimationCurve();

	private AnimationCurve _upCurve = new AnimationCurve();

	private bool _followForwardLength;

	private bool _usingGlobalUpVector = true;

	private float _rightMultiply = 1f;

	private float _upMultiply = 1f;

	string ITypeValueBase.ValuePath { get; set; }

	public BlendRelative()
	{
	}

	public BlendRelative(BlendRelative prototype)
	{
		_forwardCurve = prototype._forwardCurve;
		_rightCurve = prototype._rightCurve;
		_upCurve = prototype._upCurve;
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
		Vector3 vector = value2 - value;
		float magnitude = vector.magnitude;
		Vector3 normalized = Vector3.Cross(Vector3.up, vector).normalized;
		Vector3 vector2 = ((!_usingGlobalUpVector) ? Vector3.Cross(vector, normalized).normalized : Vector3.up);
		if (_followForwardLength)
		{
			normalized *= magnitude;
			vector2 *= magnitude;
		}
		o_value = value + vector * Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_forwardCurve, value3, value3) + normalized * _rightMultiply * Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_rightCurve, value3, 0f) + vector2 * _upMultiply * Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(_upCurve, value3, 0f);
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
		return new BlendRelative(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		_forwardCurve = JsonTools.ToCurve(json["ForwardCurve"], _forwardCurve);
		_rightCurve = JsonTools.ToCurve(json["RightCurve"], _rightCurve);
		_upCurve = JsonTools.ToCurve(json["UpCurve"], _upCurve);
		_followForwardLength = json["FollowForwardLength"].GETBOOLEAN(_followForwardLength);
		_usingGlobalUpVector = json["UsingGlobalUpVector"].GETBOOLEAN(_usingGlobalUpVector);
		_rightMultiply = json["RightMultiply"].GETFLOAT(_rightMultiply);
		_upMultiply = json["UpMultiply"].GETFLOAT(_upMultiply);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		jsonData.AddField("ForwardCurve", JsonTools.FromCurve(_forwardCurve));
		jsonData.AddField("RightCurve", JsonTools.FromCurve(_rightCurve));
		jsonData.AddField("UpCurve", JsonTools.FromCurve(_upCurve));
		jsonData.AddFieldBool("FollowForwardLength", _followForwardLength);
		jsonData.AddFieldBool("UsingGlobalUpVector", _usingGlobalUpVector);
		jsonData.AddFieldDouble("RightMultiply", _rightMultiply);
		jsonData.AddFieldDouble("UpMultiply", _upMultiply);
		return jsonData;
	}
}
