using System;
using System.Collections.Generic;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NRuntime.NVariable;
using NGame2.NNodeTree.NUtil;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree;

public class TypeFactory : SingletonAuto<TypeFactory>
{
	public class TypeData
	{
		public IValueFactory VariableValueFactory;

		public Func<ITypeValueBase, IVariable, object> ToObjectFunc;
	}

	public Dictionary<Type, TypeData> TypeDictionary = new Dictionary<Type, TypeData>();

	public List<string> TypeNameList = new List<string>();

	public TypeFactory()
	{
		AddTypeData<bool>(delegate(ITypeValueBase typeValueBase, IVariable variable)
		{
			ITypeValue<bool> typeValue = typeValueBase.Casting<bool>();
			return (typeValue == null) ? ((object)false) : ((object)typeValue.GetValue(variable));
		});
		AddTypeData<long>(delegate(ITypeValueBase typeValueBase, IVariable variable)
		{
			ITypeValue<long> typeValue = typeValueBase.Casting<long>();
			return (typeValue == null) ? ((object)0) : ((object)typeValue.GetValue(variable));
		});
		AddTypeData<float>(delegate(ITypeValueBase typeValueBase, IVariable variable)
		{
			ITypeValue<float> typeValue = typeValueBase.Casting<float>();
			return (typeValue == null) ? ((object)0f) : ((object)typeValue.GetValue(variable));
		});
		AddTypeData<Vector3>(delegate(ITypeValueBase typeValueBase, IVariable variable)
		{
			ITypeValue<Vector3> typeValue = typeValueBase.Casting<Vector3>();
			return (typeValue == null) ? ((object)Vector3.zero) : ((object)typeValue.GetValue(variable));
		});
		AddTypeData<Quaternion>(delegate(ITypeValueBase typeValueBase, IVariable variable)
		{
			ITypeValue<Quaternion> typeValue = typeValueBase.Casting<Quaternion>();
			return (typeValue == null) ? ((object)Quaternion.identity) : ((object)typeValue.GetValue(variable));
		});
		AddTypeData<Transform>((ITypeValueBase typeValueBase, IVariable variable) => typeValueBase.Casting<Transform>()?.GetValue(variable));
		AddTypeData<object>((ITypeValueBase typeValueBase, IVariable variable) => typeValueBase.Casting<object>()?.GetValue(variable));
		AddTypeData<string>(delegate(ITypeValueBase typeValueBase, IVariable variable)
		{
			ITypeValue<string> typeValue = typeValueBase.Casting<string>();
			return (typeValue == null) ? string.Empty : typeValue.GetValue(variable);
		});
		AddTypeData<decimal>(delegate(ITypeValueBase typeValueBase, IVariable variable)
		{
			ITypeValue<decimal> typeValue = typeValueBase.Casting<decimal>();
			return (typeValue == null) ? ((object)0m) : ((object)typeValue.GetValue(variable));
		});
		foreach (Type key in TypeDictionary.Keys)
		{
			TypeNameList.Add(Lookup.GetTypeName(key));
		}
	}

	private void AddTypeData<T>(Func<ITypeValueBase, IVariable, object> toObjFunc)
	{
		TypeData typeData = new TypeData();
		typeData.VariableValueFactory = new GenericValueFactory<T>();
		typeData.ToObjectFunc = toObjFunc;
		TypeDictionary.Add(typeof(T), typeData);
		typeData.VariableValueFactory.CreateValue(null, null, isDynamic: false);
	}

	public IVariableValueBase CreateVariableValue(ITypeValueBase typeValue, IVariable variable, bool isDynamic)
	{
		Type valueType = typeValue.GetValueType();
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(TypeDictionary, valueType, null)?.VariableValueFactory.CreateValue(typeValue, variable, isDynamic);
	}

	public object TypeToObject(ITypeValueBase typeValue, IVariable variable)
	{
		Type valueType = typeValue.GetValueType();
		TypeData vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(TypeDictionary, valueType, null);
		if (vALUE == null)
		{
			return string.Empty;
		}
		if (vALUE.ToObjectFunc == null)
		{
			return string.Empty;
		}
		return vALUE.ToObjectFunc(typeValue, variable);
	}
}
