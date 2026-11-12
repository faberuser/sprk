using System;
using System.Collections.Generic;
using MiniJSON;
using NShared.NJit;

namespace NShared;

public class ConstantDataContainer : IJitDataContainer, IGetData_for_GMS<string, ConstantData>, IGetAllData_for_GMS<ConstantData>
{
	public bool IsLoaded => GetDataContainer_Base().IsLoaded;

	void IJitDataContainer.Load()
	{
	}

	private ConstantDataContainer_Base GetDataContainer_Base()
	{
		ConstantDataContainer_FromPatch dataContainer = TableRoot.Instance.GetDataContainer<ConstantDataContainer_FromPatch>();
		if (dataContainer != null)
		{
			return dataContainer;
		}
		return TableRoot.Instance.GetDataContainer<ConstantDataContainer_InApp>();
	}

	public IList<ConstantData> GetAllData()
	{
		return GetDataContainer_Base().GetAllData();
	}

	public ConstantData GetData(string key)
	{
		return GetDataContainer_Base().GetData(key);
	}

	public string GetValue(string key, string defaultValue = "")
	{
		return GetDataContainer_Base().GetValue(key, defaultValue);
	}

	public int GetValueInt(string key, int defaultValue = 0)
	{
		return GetDataContainer_Base().GetValueInt(key, defaultValue);
	}

	public bool GetValueBool(string key)
	{
		return GetDataContainer_Base().GetValueBool(key);
	}

	public T GetValueEnum<T>(string key) where T : struct
	{
		string value = GetValue(key);
		if (value == null || value.Length == 0)
		{
			return default(T);
		}
		try
		{
			return EnumParser.Parse<T>(value);
		}
		catch (Exception)
		{
			return default(T);
		}
	}

	public T[] GetValueArray<T>(string key)
	{
		string value = GetValue(key);
		if (string.IsNullOrEmpty(value))
		{
			return null;
		}
		try
		{
			return ParesJsonArray<T>(value);
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static T[] ParesJsonArray<T>(string value)
	{
		if (Json.Deserialize(value) is List<object> list)
		{
			List<T> list2 = new List<T>();
			for (int i = 0; i < list.Count; i++)
			{
				T item = BaseJsonMarshaler.To<T>(list[i]);
				list2.Add(item);
			}
			return list2.ToArray();
		}
		return null;
	}
}
