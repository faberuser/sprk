using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NRuntime.NVariable;
using NVespa;

namespace NGame2.NNodeTree.NRuntime;

public class LocalVariable : SmallPooling<LocalVariable>, IVariable, ISmallPoolingItem
{
	private RuntimeInstance _runtime;

	private IVariable _parent;

	private StringKeyDictionary<IVariableValueBase> _valueDic;

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		if (_parent != null)
		{
			_parent.ReleaseSelf();
			_parent = null;
		}
		if (_valueDic != null)
		{
			_valueDic.Clear();
		}
		_runtime = null;
	}

	public void SetParent(IVariable variable)
	{
		if (_parent != null)
		{
			_parent.ReleaseSelf();
		}
		if (variable != null)
		{
			_parent = variable.CaptureSelf();
		}
	}

	public void AddValue<T>(StringKey key, T value)
	{
		if (_valueDic == null)
		{
			_valueDic = ReservePoolUtil.Create<StringKeyDictionary<IVariableValueBase>>();
		}
		_valueDic.ADD(key, ValueGenericMember<T>.Create(value));
	}

	public void AddFunc<T>(StringKey key, Func<T> func)
	{
		if (_valueDic == null)
		{
			_valueDic = ReservePoolUtil.Create<StringKeyDictionary<IVariableValueBase>>();
		}
		_valueDic.ADD(key, new ValueGenericFunc<T>(func));
	}

	public void AddVariableValue(StringKey key, IVariableValueBase valueBase)
	{
		if (_valueDic == null)
		{
			_valueDic = ReservePoolUtil.Create<StringKeyDictionary<IVariableValueBase>>();
		}
		_valueDic.ADD(key, valueBase);
	}

	public void AddTypeValue(StringKey key, ITypeValueBase typeValue, bool isDynamic)
	{
		IVariableValueBase variableValueBase = SingletonAuto<TypeFactory>.instance.CreateVariableValue(typeValue, _parent, isDynamic);
		if (variableValueBase != null)
		{
			if (_valueDic == null)
			{
				_valueDic = ReservePoolUtil.Create<StringKeyDictionary<IVariableValueBase>>();
			}
			_valueDic.ADD(key, variableValueBase);
		}
	}

	void IVariable.Open(RuntimeInstance runtime)
	{
		_runtime = runtime;
	}

	IVariable IVariable.CaptureSelf()
	{
		return SmallPooling<LocalVariable>.CaptureInstance(this);
	}

	void IVariable.ReleaseSelf()
	{
		SmallPooling<LocalVariable>.ReleaseInstance(this);
	}

	RuntimeInstance IVariable.GetRuntimeInstance()
	{
		return _runtime;
	}

	IVariableValueBase IVariable.GetValueBase(StringKey key)
	{
		if (key.IsNullOrEmpty())
		{
			return null;
		}
		if (_valueDic != null && _valueDic.TryGetValue(key, out var value))
		{
			return value;
		}
		if (_parent != null)
		{
			return _parent.GetValueBase(key);
		}
		return SingletonAuto<EnvironmentVariable>.instance.GetValueBase(key);
	}

	JsonData IVariable.DumpJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Variable Type", "LocalVariable");
		if (_valueDic != null)
		{
			foreach (KeyValuePair<StringKey, IVariableValueBase> item in _valueDic.CreateDictionary())
			{
				JsonData value = item.Value.DumpJson();
				jsonData.AddField(item.Key.Value, value);
			}
		}
		jsonData.AddField("Parent Variable", _parent.DumpJson());
		return jsonData;
	}
}
