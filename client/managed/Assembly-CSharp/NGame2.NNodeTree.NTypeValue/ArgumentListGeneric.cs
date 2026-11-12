using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue;

public class ArgumentListGeneric<T> : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	private List<ArgumentSingleGeneric<T>> _list = new List<ArgumentSingleGeneric<T>>();

	private Func<ArgumentSingleGeneric<T>> _createFunc;

	public int Count => _list.Count;

	public ArgumentSingleGeneric<T> this[int index] => Vespa_Generic_IList_Extension_Method.GetValid(_list, index);

	public ArgumentListGeneric(T value)
	{
		_createFunc = () => new ArgumentSingleGeneric<T>(value);
	}

	public ArgumentListGeneric(StringKey variableName)
	{
		_createFunc = () => new ArgumentSingleGeneric<T>(variableName);
	}

	public void Wakeup(IVariable variable)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
		{
			_list[i].GetValue(variable);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsArray())
		{
			return false;
		}
		for (int i = 0; i < json.Count; i++)
		{
			ArgumentSingleGeneric<T> argumentSingleGeneric = Vespa_System_Action_Extension_Method.fire(_createFunc);
			argumentSingleGeneric.LoadFromJson(json.GetFromArray(i));
			_list.Add(argumentSingleGeneric);
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		for (int i = 0; i < _list.Count; i++)
		{
			jsonData.Add(_list[i].MakeJson());
		}
		return jsonData;
	}
}
