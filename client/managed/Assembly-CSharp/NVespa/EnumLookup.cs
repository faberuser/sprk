using System;
using System.Collections.Generic;

namespace NVespa;

public class EnumLookup<EnumType> : SingletonAuto<EnumLookup<EnumType>>, IEnumLookup
{
	public class ValueData
	{
		public string Name;

		public Enum SystemValue;

		public int HashCode;

		public EnumType EnumValue;
	}

	public static bool IsFlags;

	public static Type Type;

	private ValueData _default;

	private string[] _nameArray;

	private ValueData[] _valueArray;

	private Dictionary<string, ValueData> _nameDic;

	private Dictionary<EnumType, ValueData> _enumDic;

	private Dictionary<Enum, ValueData> _systemDic;

	protected override bool ThreadSafe => true;

	public static ValueData Default
	{
		get
		{
			lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
			{
				return SingletonAuto<EnumLookup<EnumType>>.instance._default;
			}
		}
	}

	public static string[] NameArray
	{
		get
		{
			lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
			{
				return SingletonAuto<EnumLookup<EnumType>>.instance._nameArray;
			}
		}
	}

	public static ValueData[] ValueArray
	{
		get
		{
			lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
			{
				return SingletonAuto<EnumLookup<EnumType>>.instance._valueArray;
			}
		}
	}

	public static bool TryGetByName(string name, out ValueData value)
	{
		if (name == null)
		{
			value = null;
			return false;
		}
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			return SingletonAuto<EnumLookup<EnumType>>.instance._nameDic.TryGetValue(name, out value);
		}
	}

	public static ValueData GetByName(string name, ValueData defaultValue)
	{
		if (name == null)
		{
			return defaultValue;
		}
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(SingletonAuto<EnumLookup<EnumType>>.instance._nameDic, name, defaultValue);
		}
	}

	public static ValueData GetByName(string name)
	{
		if (name == null)
		{
			return Default;
		}
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(SingletonAuto<EnumLookup<EnumType>>.instance._nameDic, name, Default);
		}
	}

	public static bool TryGetByEnum(EnumType enumValue, out ValueData value)
	{
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			return SingletonAuto<EnumLookup<EnumType>>.instance._enumDic.TryGetValue(enumValue, out value);
		}
	}

	public static ValueData GetByEnum(EnumType enumValue, ValueData defaultValue)
	{
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(SingletonAuto<EnumLookup<EnumType>>.instance._enumDic, enumValue, defaultValue);
		}
	}

	public static ValueData GetByEnum(EnumType enumValue)
	{
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(SingletonAuto<EnumLookup<EnumType>>.instance._enumDic, enumValue, Default);
		}
	}

	public static bool TryGetBySystem(Enum systemEnum, out ValueData value)
	{
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			return SingletonAuto<EnumLookup<EnumType>>.instance._systemDic.TryGetValue(systemEnum, out value);
		}
	}

	public static ValueData GetBySystem(Enum systemEnum, ValueData defaultValue)
	{
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(SingletonAuto<EnumLookup<EnumType>>.instance._systemDic, systemEnum, defaultValue);
		}
	}

	public static ValueData GetBySystem(Enum systemEnum)
	{
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(SingletonAuto<EnumLookup<EnumType>>.instance._systemDic, systemEnum, Default);
		}
	}

	public static EnumType Parse(string i_str, EnumType defaultValue = default(EnumType))
	{
		if (!TryParse(i_str, out var o_value))
		{
			return defaultValue;
		}
		return o_value;
	}

	public static bool TryParse(string i_str, out EnumType o_value)
	{
		if (String_Extension_Method.IsNullOrEmpty(i_str))
		{
			o_value = default(EnumType);
			return false;
		}
		if (IsFlags)
		{
			return TryParseFlags(i_str, out o_value);
		}
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			ValueData vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(SingletonAuto<EnumLookup<EnumType>>.instance._nameDic, i_str, null);
			if (vALUE == null)
			{
				o_value = default(EnumType);
				return false;
			}
			o_value = vALUE.EnumValue;
			return true;
		}
	}

	public static EnumType ParseFlags(string i_str, EnumType defaultValue = default(EnumType))
	{
		if (!TryParseFlags(i_str, out var o_value))
		{
			return defaultValue;
		}
		return o_value;
	}

	public static bool TryParseFlags(string i_str, out EnumType o_value)
	{
		try
		{
			o_value = (EnumType)Enum.Parse(typeof(EnumType), i_str);
			return true;
		}
		catch (Exception)
		{
		}
		o_value = default(EnumType);
		return false;
	}

	static EnumLookup()
	{
		Initialize();
	}

	public static void Initialize()
	{
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			SingletonAuto<EnumLookup<EnumType>>.CreateInstance();
		}
	}

	public EnumLookup()
	{
		string fullName = typeof(EnumType).FullName;
		IEqualityComparer<EnumType> comparer = SingletonAuto<EqualityComparerCache>.instance.Get<EnumType>();
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			_nameDic = new Dictionary<string, ValueData>();
			_systemDic = new Dictionary<Enum, ValueData>();
			_enumDic = new Dictionary<EnumType, ValueData>(comparer);
			IsFlags = Attribute.GetCustomAttribute(typeof(EnumType), typeof(FlagsAttribute)) != null;
			_nameArray = Enum.GetNames(typeof(EnumType));
			Array values = Enum.GetValues(typeof(EnumType));
			_valueArray = new ValueData[_nameArray.Length];
			for (int i = 0; i < _nameArray.Length && i < values.Length; i++)
			{
				ValueData valueData = new ValueData();
				valueData.Name = _nameArray[i];
				valueData.EnumValue = (EnumType)values.GetValue(i);
				valueData.SystemValue = (Enum)values.GetValue(i);
				valueData.HashCode = valueData.EnumValue.GetHashCode();
				_valueArray[i] = valueData;
				if (!_nameDic.ContainsKey(valueData.Name))
				{
					_nameDic.Add(valueData.Name, valueData);
				}
				if (_enumDic.ContainsKey(valueData.EnumValue))
				{
					if (fullName.StartsWith("UnityEngine."))
					{
					}
				}
				else
				{
					_enumDic.Add(valueData.EnumValue, valueData);
				}
				if (_systemDic.ContainsKey(valueData.SystemValue))
				{
					if (fullName.StartsWith("UnityEngine."))
					{
					}
				}
				else
				{
					_systemDic.Add(valueData.SystemValue, valueData);
				}
			}
			_default = new ValueData();
			_default.EnumValue = (EnumType)values.GetValue(0);
			_default.SystemValue = (Enum)values.GetValue(0);
			_default.Name = _nameArray[0];
			_default.HashCode = _default.EnumValue.GetHashCode();
		}
	}

	void IEnumLookup.ReleaseSelf()
	{
		lock (SingletonAuto<EnumLookup<EnumType>>.LockObj)
		{
			_default = null;
			_nameArray = null;
			_valueArray = null;
			_nameDic = null;
			_enumDic = null;
			_systemDic = null;
			SingletonAuto<EnumLookup<EnumType>>.ReleaseSingleton();
			SingletonAuto<EnumLookup<EnumType>>.CreateInstance();
		}
	}
}
