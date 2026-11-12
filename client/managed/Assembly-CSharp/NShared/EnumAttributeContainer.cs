using System;
using System.Collections.Generic;
using System.Reflection;
using NVespa;

namespace NShared;

public static class EnumAttributeContainer<TEnumType, TAttributeType> where TEnumType : struct where TAttributeType : class
{
	private static bool _init;

	private static Dictionary<TEnumType, TAttributeType> _dictionary;

	static EnumAttributeContainer()
	{
		_init = false;
		_dictionary = new Dictionary<TEnumType, TAttributeType>();
		Init();
	}

	public static void Init()
	{
		if (_init)
		{
			return;
		}
		_init = true;
		Type typeFromHandle = typeof(TEnumType);
		foreach (TEnumType value2 in Enum.GetValues(typeFromHandle))
		{
			MemberInfo[] member = typeFromHandle.GetMember(value2.ToString());
			if (member == null || member.Length == 0)
			{
				continue;
			}
			object[] customAttributes = member[0].GetCustomAttributes(typeof(TAttributeType), inherit: false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				if (customAttributes.Length >= 2)
				{
					Action<object> logError = VespaDebug.LogError;
					TEnumType val2 = value2;
					logError("Duplicated FunctionalityType Attribute : " + val2);
				}
				TAttributeType value = customAttributes[0] as TAttributeType;
				_dictionary.Add(value2, value);
			}
		}
	}

	public static void ForEach(Action<TEnumType, TAttributeType> action)
	{
		foreach (KeyValuePair<TEnumType, TAttributeType> item in _dictionary)
		{
			action(item.Key, item.Value);
		}
	}

	public static TAttributeType Get(TEnumType type)
	{
		TAttributeType value = null;
		_dictionary.TryGetValue(type, out value);
		return value;
	}

	public static bool Has(TEnumType type)
	{
		return Get(type) != null;
	}
}
