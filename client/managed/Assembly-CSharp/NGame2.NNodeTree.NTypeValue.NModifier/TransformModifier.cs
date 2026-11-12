using LitJson;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NSceneContainer;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NModifier;

public class TransformModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public interface IItem
	{
		Vector3 PrevPosition { get; set; }
	}

	public enum EModifierType
	{
		Disabled,
		InitTransform,
		UpdateTransform
	}

	public enum ERotationType
	{
		None,
		Billboard,
		VerticalBillboard,
		FollowLocalPosition,
		FollowWorldPosition
	}

	private EModifierType _modifierType;

	private ArgumentSingleGeneric<Vector3> _position;

	private ArgumentSingleGeneric<Quaternion> _rotation;

	private ArgumentSingleGeneric<Vector3> _scale;

	private ArgumentSingleGeneric<bool> _isFlip;

	private ERotationType _rotationType;

	public TransformModifier()
	{
		_position = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
		_rotation = new ArgumentSingleGeneric<Quaternion>(Quaternion.identity);
		_scale = new ArgumentSingleGeneric<Vector3>(Vector3.one);
		_isFlip = new ArgumentSingleGeneric<bool>(value: false);
	}

	public void Wakeup(IVariable variable)
	{
		_position.GetValue(variable);
		_rotation.GetValue(variable);
		_scale.GetValue(variable);
		_isFlip.GetValue(variable);
	}

	public void InitTransform(Transform transform, IVariable variable, IItem item)
	{
		if (_modifierType == EModifierType.Disabled)
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
		}
		else
		{
			if (_modifierType != EModifierType.InitTransform && _modifierType != EModifierType.UpdateTransform)
			{
				return;
			}
			UpdateTransformInternal(transform, variable, null);
			if (item != null)
			{
				if (_rotationType == ERotationType.FollowLocalPosition)
				{
					item.PrevPosition = transform.localPosition;
				}
				else if (_rotationType == ERotationType.FollowWorldPosition)
				{
					item.PrevPosition = transform.position;
				}
			}
		}
	}

	public void UpdateTransform(Transform transform, IVariable variable, IItem item)
	{
		if (_modifierType != EModifierType.Disabled && _modifierType != EModifierType.InitTransform && _modifierType == EModifierType.UpdateTransform)
		{
			UpdateTransformInternal(transform, variable, item);
		}
	}

	public void UpdateTransformInternal(Transform transform, IVariable variable, IItem item)
	{
		UpdateTransformPosition(transform, variable);
		UpdateTransformRotation(transform, variable, item);
		UpdateTransformScale(transform, variable);
	}

	private void UpdateTransformPosition(Transform transform, IVariable variable)
	{
		if (!_position.TryGetValue(variable, out var value))
		{
			value = Vector3.zero;
		}
		transform.localPosition = value;
	}

	private void UpdateTransformRotation(Transform transform, IVariable variable, IItem item)
	{
		if (!_rotation.TryGetValue(variable, out var value))
		{
			value = Quaternion.identity;
		}
		if (_isFlip.GetValue(variable))
		{
			value *= Quaternion.AngleAxis(180f, Vector3.right);
		}
		if (_rotationType != ERotationType.None)
		{
			if (_rotationType == ERotationType.Billboard)
			{
				ICamera cAMERA = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETCAMERA();
				if (cAMERA != null)
				{
					transform.rotation = cAMERA.GetUnityCamera().transform.rotation * value;
					return;
				}
			}
			else if (_rotationType == ERotationType.VerticalBillboard)
			{
				ICamera cAMERA2 = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETCAMERA();
				if (cAMERA2 != null)
				{
					transform.rotation = Math.CalcVerticalBillboard(cAMERA2.GetUnityCamera()) * value;
					return;
				}
			}
			else if (_rotationType == ERotationType.FollowLocalPosition)
			{
				if (item != null)
				{
					Vector3 vector = transform.localPosition - item.PrevPosition;
					if (!(vector.magnitude < 0.01f))
					{
						item.PrevPosition = transform.localPosition;
						transform.localRotation = Quaternion.LookRotation(vector.normalized) * value;
					}
					return;
				}
			}
			else
			{
				if (_rotationType != ERotationType.FollowWorldPosition)
				{
					return;
				}
				if (item != null)
				{
					Vector3 vector2 = transform.position - item.PrevPosition;
					if (!(vector2.magnitude < Mathf.Epsilon))
					{
						item.PrevPosition = transform.position;
						transform.rotation = Quaternion.LookRotation(vector2.normalized) * value;
					}
					return;
				}
			}
		}
		transform.localRotation = value;
	}

	private void UpdateTransformScale(Transform transform, IVariable variable)
	{
		Vector3 value = _scale.GetValue(variable, Vector3.one);
		if (!_isFlip.GetValue(variable))
		{
			transform.localScale = value;
		}
		else
		{
			transform.localScale = -value;
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		_modifierType = json["ModifierType"].GETENUM(_modifierType);
		_position.LoadFromJson(json.GetJsonAny("Position", "LocalPosition"));
		_rotation.LoadFromJson(json.GetJsonAny("Rotation", "LocalRotation"));
		_scale.LoadFromJson(json.GetJsonAny("Scale", "LocalScale"));
		_isFlip.LoadFromJson(json["IsFlip"]);
		_rotationType = json["RotationType"].GETENUM(_rotationType);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldEnum("ModifierType", _modifierType);
		jsonData.AddField("Position", _position.MakeJson());
		jsonData.AddField("Rotation", _rotation.MakeJson());
		jsonData.AddField("Scale", _scale.MakeJson());
		jsonData.AddField("IsFlip", _isFlip.MakeJson());
		jsonData.AddFieldEnum("RotationType", _rotationType);
		return jsonData;
	}
}
