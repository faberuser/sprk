using System;
using LitJson;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NRender;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NVector3;

public class GetPosition : ITypeValue<Vector3>, ITypeValueBase
{
	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public GetPosition()
	{
	}

	public GetPosition(GetPosition prototype)
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
		Transform transform = GetTransform(variable);
		if (transform == null)
		{
			o_value = Vector3.zero;
			return true;
		}
		o_value = transform.position;
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
		return new GetPosition(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		_argumentContainer.LoadFromJson(json, null);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		return _argumentContainer.MakeJson(null);
	}

	private Transform GetTransform(IVariable variable)
	{
		ITypeValue<long> typeValue = _argumentContainer.GetTypeValue<long>(0);
		if (typeValue != null)
		{
			long value = typeValue.GetValue(variable);
			return GetTransformFromUid(variable, value);
		}
		ITypeValue<object> typeValue2 = _argumentContainer.GetTypeValue<object>(0);
		if (typeValue2 != null)
		{
			Component component = typeValue2.GetValue(variable) as Component;
			if (!(component == null))
			{
				return component.transform;
			}
			return null;
		}
		return _argumentContainer.GetTypeValue<Transform>(0)?.GetValue(variable);
	}

	private Transform GetTransformFromUid(IVariable variable, long uid)
	{
		RenderFacade rENDERFACADE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETRENDERFACADE((int)uid);
		if (rENDERFACADE != null)
		{
			return rENDERFACADE.transform;
		}
		ObjectBase objectBase = Container.instance.GetObjectBase(uid);
		if (objectBase != null)
		{
			return objectBase.transform;
		}
		return null;
	}
}
