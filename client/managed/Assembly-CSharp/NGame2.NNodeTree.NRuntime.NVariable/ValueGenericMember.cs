using System;
using LitJson;
using NVespa;

namespace NGame2.NNodeTree.NRuntime.NVariable;

public class ValueGenericMember<T> : IVariableValue<T>, IVariableValueBase
{
	public T Value;

	public static ValueGenericMember<T> Create(T value)
	{
		ValueGenericMember<T> valueGenericMember = ReservePoolUtil.Create<ValueGenericMember<T>>();
		valueGenericMember.Value = value;
		return valueGenericMember;
	}

	T IVariableValue<T>.GetValue()
	{
		return Value;
	}

	Type IVariableValueBase.GetValueType()
	{
		return typeof(T);
	}

	JsonData IVariableValueBase.DumpJson()
	{
		return new JsonData(Value.ToString());
	}
}
