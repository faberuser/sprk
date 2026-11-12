using LitJson;
using NGame2.NNodeTree.NRuntime;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NModifier;

public class TransformGet : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	private bool _isEnabled;

	private ArgumentSingleGeneric<Vector3> _position;

	private ArgumentSingleGeneric<Quaternion> _rotation;

	private ArgumentSingleGeneric<float> _scale;

	private ArgumentSingleGeneric<bool> _isFlip;

	public TransformGet()
	{
		_position = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
		_rotation = new ArgumentSingleGeneric<Quaternion>(Quaternion.identity);
		_scale = new ArgumentSingleGeneric<float>(1f);
		_isFlip = new ArgumentSingleGeneric<bool>(value: false);
	}

	public void Wakeup(IVariable variable)
	{
		_position.GetValue(variable);
		_rotation.GetValue(variable);
		_scale.GetValue(variable);
		_isFlip.GetValue(variable);
	}

	public void GetTransformValue(IVariable variable, out Vector3 oPosition, out Quaternion oRotation, out float oScale)
	{
		if (!_isEnabled)
		{
			oPosition = Vector3.zero;
			oRotation = Quaternion.identity;
			oScale = 1f;
			return;
		}
		oPosition = _position.GetValue(variable, Vector3.zero);
		oRotation = _rotation.GetValue(variable, Quaternion.identity);
		oScale = _scale.GetValue(variable, 1f);
		if (_isFlip.GetValue(variable))
		{
			oRotation *= Quaternion.AngleAxis(180f, Vector3.right);
			oScale = 0f - oScale;
		}
	}

	public ITransformData GetTransformData(IVariable variable)
	{
		if (!_isEnabled)
		{
			return null;
		}
		TransformData2 transformData = new TransformData2();
		transformData.Position = _position.GetValue(variable, Vector3.zero);
		transformData.Rotation = _rotation.GetValue(variable, Quaternion.identity);
		transformData.Scale = _scale.GetValue(variable, 1f);
		if (_isFlip.GetValue(variable))
		{
			transformData.Rotation *= Quaternion.AngleAxis(180f, Vector3.right);
			transformData.Scale = 0f - transformData.Scale;
		}
		return transformData;
	}

	bool ILoadJson.Load(JsonData json)
	{
		_isEnabled = json["IsEnabled"].GETBOOLEAN(_isEnabled);
		_position.LoadFromJson(json.GetJsonAny("Position", "LocalPosition"));
		_rotation.LoadFromJson(json.GetJsonAny("Rotation", "LocalRotation"));
		_scale.LoadFromJson(json.GetJsonAny("Scale", "LocalScale"));
		_isFlip.LoadFromJson(json["IsFlip"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnabled", _isEnabled);
		jsonData.AddField("Position", _position.MakeJson());
		jsonData.AddField("Rotation", _rotation.MakeJson());
		jsonData.AddField("Scale", _scale.MakeJson());
		jsonData.AddField("IsFlip", _isFlip.MakeJson());
		return jsonData;
	}
}
