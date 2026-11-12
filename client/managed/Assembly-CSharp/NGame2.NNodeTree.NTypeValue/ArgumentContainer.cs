using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NRuntime;
using NVespa;

namespace NGame2.NNodeTree.NTypeValue;

public class ArgumentContainer
{
	public List<ArgumentHolder> HolderList { get; private set; }

	public void AppendTypeValue(ITypeValueBase append)
	{
		if (HolderList == null)
		{
			HolderList = new List<ArgumentHolder>();
		}
		HolderList.Add(new ArgumentHolder(append));
	}

	public ITypeValueBase GetTypeValueBase(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(HolderList, index))
		{
			return null;
		}
		return HolderList[index].Value;
	}

	public ITypeValue<T> GetTypeValue<T>(int index)
	{
		return GetTypeValueBase(index).Casting<T>();
	}

	public T GetValue<T>(IVariable variable, int index)
	{
		ITypeValue<T> typeValue = GetTypeValue<T>(index);
		if (typeValue == null)
		{
			return default(T);
		}
		return typeValue.GetValue(variable);
	}

	public bool LoadFromJson(JsonData json, Action<ArgumentHolder, JsonData> actionOnLoad)
	{
		if (!json.IsArray())
		{
			return true;
		}
		if (json.Count == 0)
		{
			return true;
		}
		HolderList = new List<ArgumentHolder>();
		for (int i = 0; i < json.Count; i++)
		{
			JsonData fromArray = json.GetFromArray(i);
			ITypeValueBase typeValueBase = SingletonAuto<ValueFactory>.instance.CreateFromJson(fromArray);
			if (typeValueBase != null)
			{
				ArgumentHolder argumentHolder = new ArgumentHolder(typeValueBase);
				Vespa_System_Action_Extension_Method.fire(actionOnLoad, argumentHolder, fromArray);
				HolderList.Add(argumentHolder);
			}
		}
		return true;
	}

	public JsonData MakeJson(Action<ArgumentHolder, JsonData> actionOnMake)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(HolderList))
		{
			return null;
		}
		JsonData jsonData = new JsonData();
		for (int i = 0; i < HolderList.Count; i++)
		{
			ArgumentHolder argumentHolder = HolderList[i];
			JsonData jsonData2 = SingletonAuto<ValueFactory>.instance.MakeJson(argumentHolder.Value);
			Vespa_System_Action_Extension_Method.fire(actionOnMake, argumentHolder, jsonData2);
			jsonData.Add(jsonData2);
		}
		return jsonData;
	}
}
