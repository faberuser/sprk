using System;
using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NRender;
using NGameAsset;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NTransform;

public class FindFieldMapTransform : ITypeValue<Transform>, ITypeValueBase
{
	private class NodeNameData
	{
		private string _value = string.Empty;

		public EPivotType PivotType { get; private set; }

		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (!(_value == value))
				{
					_value = value;
					PivotType = Vespa_String_Extension_Method.TOENUM(_value, EPivotType.__NoPivot__);
				}
			}
		}

		public NodeNameData()
		{
			PivotType = EPivotType.__NoPivot__;
			_value = string.Empty;
		}

		public NodeNameData(NodeNameData protoType)
		{
			PivotType = protoType.PivotType;
			_value = protoType._value;
		}
	}

	public bool SkipNullCaching;

	private NodeNameData _nodeName = new NodeNameData();

	private ArgumentContainer _argumentContainer = new ArgumentContainer();

	string ITypeValueBase.ValuePath { get; set; }

	public FindFieldMapTransform()
	{
	}

	public FindFieldMapTransform(FindFieldMapTransform prototype)
	{
		_nodeName = new NodeNameData(prototype._nodeName);
	}

	Transform ITypeValue<Transform>.GetValue(IVariable variable)
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((ITypeValue<Transform>)this).TryGetValue(variable, out var o_value))
		{
			return null;
		}
		return o_value;
	}

	bool ITypeValue<Transform>.TryGetValue(IVariable variable, out Transform o_value)
	{
		o_value = GetValueInternal(variable);
		if (o_value == null)
		{
			return false;
		}
		return true;
	}

	private Transform GetValueInternal(IVariable variable)
	{
		ITypeValue<long> typeValue = _argumentContainer.GetTypeValue<long>(0);
		if (typeValue != null)
		{
			Transform transformFromUid = GetTransformFromUid(variable, typeValue);
			if (transformFromUid != null)
			{
				return transformFromUid;
			}
		}
		ITypeValue<object> typeValue2 = _argumentContainer.GetTypeValue<object>(0);
		if (typeValue2 != null)
		{
			Component component = typeValue2.GetValue(variable) as Component;
			if (component != null)
			{
				Transform transform = FindTransformRecursively(component.transform, _nodeName.Value);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		ITypeValue<Transform> typeValue3 = _argumentContainer.GetTypeValue<Transform>(0);
		if (typeValue3 != null)
		{
			Transform value = typeValue3.GetValue(variable);
			Transform transform2 = FindTransformRecursively(value, _nodeName.Value);
			if (transform2 != null)
			{
				return transform2;
			}
		}
		return null;
	}

	Type ITypeValueBase.GetValueType()
	{
		return typeof(Transform);
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
		return new FindFieldMapTransform(this);
	}

	bool ITypeValueBase.LoadFromBodyJson(JsonData json)
	{
		SkipNullCaching = json["SkipNullCaching"].GETBOOLEAN(SkipNullCaching);
		_nodeName.Value = json["NodeName"].GETSTRING(_nodeName.Value);
		_argumentContainer.LoadFromJson(json["ArgumentContainer"], null);
		return true;
	}

	JsonData ITypeValueBase.MakeBodyJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("SkipNullCaching", SkipNullCaching);
		jsonData.AddFieldString("NodeName", _nodeName.Value);
		jsonData.AddField("ArgumentContainer", _argumentContainer.MakeJson(null));
		return jsonData;
	}

	private Transform GetTransformFromUid(IVariable variable, ITypeValue<long> typeValue)
	{
		Transform fieldMapCreatureTransform = GetFieldMapCreatureTransform(variable, typeValue.GetValue(variable));
		if (fieldMapCreatureTransform != null)
		{
			return fieldMapCreatureTransform;
		}
		fieldMapCreatureTransform = GetObjectTransform(typeValue.GetValue(variable));
		if (fieldMapCreatureTransform != null)
		{
			return fieldMapCreatureTransform;
		}
		return null;
	}

	private Transform GetFieldMapCreatureTransform(IVariable variable, long uid)
	{
		if (!BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
		{
			return null;
		}
		RenderFacade renderFacade = BattleViewGenericSingleton<FieldMapManager2>.instance.GetRenderFacade(uid);
		if (renderFacade == null)
		{
			return null;
		}
		if (_nodeName.PivotType != EPivotType.__NoPivot__)
		{
			return renderFacade.Pivot.FindTransform(_nodeName.PivotType);
		}
		return renderFacade.Pivot.FindTransform(_nodeName.Value, SkipNullCaching);
	}

	private Transform GetObjectTransform(long uid)
	{
		ObjectBase objectBase = Container.instance.GetObjectBase(uid);
		if (objectBase == null)
		{
			return null;
		}
		return objectBase.FindTransform(_nodeName.Value);
	}

	private Transform FindTransformRecursively(Transform tm, string name)
	{
		if (tm == null)
		{
			return null;
		}
		if (tm.name.Equals(name) || String_Extension_Method.IsNullOrEmpty(name))
		{
			return tm;
		}
		for (int i = 0; i < tm.childCount; i++)
		{
			Transform child = tm.GetChild(i);
			Transform transform = FindTransformRecursively(child, name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}
}
