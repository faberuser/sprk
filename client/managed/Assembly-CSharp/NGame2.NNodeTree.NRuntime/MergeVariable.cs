using System.Collections.Generic;
using LitJson;
using NVespa;

namespace NGame2.NNodeTree.NRuntime;

public class MergeVariable : SmallPooling<MergeVariable>, IVariable, ISmallPoolingItem
{
	private RuntimeInstance _runtime;

	private List<IVariable> _variableList = new List<IVariable>();

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
		for (int i = 0; i < _variableList.Count; i++)
		{
			_variableList[i].ReleaseSelf();
		}
		_variableList.Clear();
		_runtime = null;
	}

	public void Add(IVariable variable)
	{
		_variableList.Add(variable.CaptureSelf());
	}

	void IVariable.Open(RuntimeInstance runtime)
	{
		_runtime = runtime;
	}

	IVariable IVariable.CaptureSelf()
	{
		return SmallPooling<MergeVariable>.CaptureInstance(this);
	}

	void IVariable.ReleaseSelf()
	{
		SmallPooling<MergeVariable>.ReleaseInstance(this);
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
		for (int i = 0; i < _variableList.Count; i++)
		{
			IVariableValueBase valueBase = _variableList[i].GetValueBase(key);
			if (valueBase != null)
			{
				return valueBase;
			}
		}
		return SingletonAuto<EnvironmentVariable>.instance.GetValueBase(key);
	}

	JsonData IVariable.DumpJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Variable Type", "MergeVariable");
		for (int i = 0; i < _variableList.Count; i++)
		{
			jsonData.AddField($"Merge {i}", _variableList[i].DumpJson());
		}
		return jsonData;
	}
}
