using System;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NUtil;

public static class Lookup
{
	public class TypeData
	{
		public string Name;

		public string ConstantName;

		public string VariableName;
	}

	private static Dictionary<Type, TypeData> _dic = new Dictionary<Type, TypeData>();

	public static TypeData GetTypeData<T>()
	{
		return GetTypeData(typeof(T));
	}

	public static TypeData GetTypeData(Type typeInfo)
	{
		if (_dic.TryGetValue(typeInfo, out var value))
		{
			return value;
		}
		value = new TypeData();
		if (typeInfo.Equals(typeof(void)))
		{
			value.Name = "none";
		}
		else if (typeInfo.Equals(typeof(float)))
		{
			value.Name = EnumTools.GetName(EVariableType.Float);
		}
		else if (typeInfo.Equals(typeof(long)))
		{
			value.Name = EnumTools.GetName(EVariableType.Uid);
		}
		else
		{
			value.Name = typeInfo.Name;
		}
		value.ConstantName = value.Name + "/Constant";
		value.VariableName = value.Name + "/Variable";
		_dic.Add(typeInfo, value);
		return value;
	}

	public static string GetTypeName<T>()
	{
		return GetTypeName(typeof(T));
	}

	public static string GetTypeName(Type typeInfo)
	{
		return GetTypeData(typeInfo).Name;
	}

	public static Color GetTypeColor<T>()
	{
		return GetTypeColor(typeof(T));
	}

	public static Color GetTypeColor(Type typeInfo)
	{
		return Color.white;
	}
}
