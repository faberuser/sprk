using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson.NInternal;
using NVespa;

namespace LitJson;

public class JsonData : IEquatable<JsonData>
{
	public interface IJsonValue
	{
		JsonType GetJsonType();

		bool EqualsJsonValue(IJsonValue value);

		void ReleaseSelf();
	}

	private IJsonValue _value;

	public string valueStr => ToString();

	public bool valueBool
	{
		get
		{
			if (_value is JsonValueBool jsonValueBool)
			{
				return jsonValueBool.Value;
			}
			if (_value is JsonValueString jsonValueString && bool.TryParse(jsonValueString.Value, out var result))
			{
				return result;
			}
			return false;
		}
	}

	public int valueInt
	{
		get
		{
			if (_value is JsonValueLong jsonValueLong)
			{
				return (int)jsonValueLong.Value;
			}
			if (_value is JsonValueString jsonValueString && int.TryParse(jsonValueString.Value, out var result))
			{
				return result;
			}
			return 0;
		}
	}

	public long valueLong
	{
		get
		{
			if (_value is JsonValueLong jsonValueLong)
			{
				return jsonValueLong.Value;
			}
			if (_value is JsonValueString jsonValueString && long.TryParse(jsonValueString.Value, out var result))
			{
				return result;
			}
			return 0L;
		}
	}

	public double valueDouble
	{
		get
		{
			if (_value is JsonValueDouble jsonValueDouble)
			{
				return jsonValueDouble.Value;
			}
			if (_value is JsonValueString jsonValueString && double.TryParse(jsonValueString.Value, out var result))
			{
				return result;
			}
			return 0.0;
		}
	}

	public int Count
	{
		get
		{
			if (_value is JsonValueArray jsonValueArray)
			{
				return __Extension_Method_ICollection__.COUNT(jsonValueArray.Value);
			}
			if (_value is JsonValueObject jsonValueObject)
			{
				return jsonValueObject.Count;
			}
			return 0;
		}
	}

	public JsonData this[string prop_name]
	{
		get
		{
			if (String_Extension_Method.IsNullOrEmpty(prop_name))
			{
				return null;
			}
			if (!(_value is JsonValueObject jsonValueObject))
			{
				return null;
			}
			return jsonValueObject.GetValue(prop_name);
		}
	}

	public static JsonData CreateJsonBool(bool value)
	{
		return SingletonAuto<JsonPool>.instance.CreateJsonBool(value);
	}

	public static JsonData CreateJsonDouble(double value)
	{
		return SingletonAuto<JsonPool>.instance.CreateJsonDouble(value);
	}

	public static JsonData CreateJsonLong(long value)
	{
		return SingletonAuto<JsonPool>.instance.CreateJsonLong(value);
	}

	public static JsonData CreateJsonString(string value)
	{
		return SingletonAuto<JsonPool>.instance.CreateJsonString(value);
	}

	public static JsonData CreateJsonData()
	{
		return SingletonAuto<JsonPool>.instance.CreateJsonData();
	}

	public void AddFieldBool(string key, bool value)
	{
		AddField(key, CreateJsonBool(value));
	}

	public void AddFieldDouble(string key, double value)
	{
		AddField(key, CreateJsonDouble(value));
	}

	public void AddFieldLong(string key, long value)
	{
		AddField(key, CreateJsonLong(value));
	}

	public void AddFieldUlong(string key, ulong value)
	{
		AddFieldString(key, value.ToString());
	}

	public void AddFieldEnum<T>(string key, T value)
	{
		AddFieldString(key, EnumTools.GetName(value));
	}

	public void AddFieldDateTime(string key, DateTime dateTime)
	{
		AddFieldString(key, dateTime.ToString());
	}

	public void AddFieldString(string key, string value)
	{
		AddField(key, CreateJsonString(value));
	}

	public void AddFieldJsonMake(string key, IMakeJson build)
	{
		AddField(key, build.MakeJson());
	}

	public void AddFieldList<T>(string key, IList<T> list) where T : IMakeJson
	{
		AddField(key, JsonTools.FromList(list));
	}

	public void AddFieldList<T>(string key, ref PoolingList<T> list) where T : IMakeJson
	{
		List<T> list2 = list.PopList();
		AddField(key, JsonTools.FromList(list2));
		list.PushList(list2);
	}

	public void AddField(string i_key, JsonData value)
	{
		if (GetJsonType() != JsonType.Object)
		{
			SetJsonValueObject();
		}
		if (_value is JsonValueObject jsonValueObject)
		{
			jsonValueObject.Add(i_key, value);
		}
	}

	public Dictionary<string, JsonData> ToDictionary()
	{
		Dictionary<string, JsonData> dictionary = new Dictionary<string, JsonData>();
		ToDictionary(dictionary);
		return dictionary;
	}

	public void ToDictionary(Dictionary<string, JsonData> i_dic)
	{
		if (_value is JsonValueObject jsonValueObject)
		{
			jsonValueObject.ToDictionary(i_dic);
		}
	}

	public IJsonValue GetRawValue()
	{
		return _value;
	}

	public JsonData GetFromArray(int index)
	{
		if (!(_value is JsonValueArray jsonValueArray))
		{
			return null;
		}
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(jsonValueArray.Value, index))
		{
			return null;
		}
		return jsonValueArray.Value[index];
	}

	public JsonData()
	{
	}

	public JsonData(bool boolean)
	{
		SetBoolean(boolean);
	}

	public JsonData(double number)
	{
		SetDouble(number);
	}

	public JsonData(int number)
	{
		SetLong(number);
	}

	public JsonData(long number)
	{
		SetLong(number);
	}

	public JsonData(object obj)
	{
		if (obj is bool)
		{
			SetBoolean((bool)obj);
		}
		else if (obj is double)
		{
			SetDouble((double)obj);
		}
		else if (obj is int)
		{
			SetLong((int)obj);
		}
		else if (obj is long)
		{
			SetLong((long)obj);
		}
		else if (obj is string)
		{
			SetString((string)obj);
		}
		else
		{
			SetNone();
		}
	}

	public JsonData(string str)
	{
		SetString(str);
	}

	public void SetNone()
	{
		SingletonAuto<JsonPool>.instance.ReleaseJsonValue(_value);
		_value = null;
	}

	public void SetBoolean(bool val)
	{
		SingletonAuto<JsonPool>.instance.ReleaseJsonValue(_value);
		_value = SingletonAuto<JsonPool>.instance.CreateJsonValueBool(val);
	}

	public void SetDouble(double val)
	{
		SingletonAuto<JsonPool>.instance.ReleaseJsonValue(_value);
		_value = SingletonAuto<JsonPool>.instance.CreateJsonValueDouble(val);
	}

	public void SetLong(long val)
	{
		SingletonAuto<JsonPool>.instance.ReleaseJsonValue(_value);
		_value = SingletonAuto<JsonPool>.instance.CreateJsonValueLong(val);
	}

	public void SetString(string val)
	{
		SingletonAuto<JsonPool>.instance.ReleaseJsonValue(_value);
		_value = SingletonAuto<JsonPool>.instance.CreateJsonValueString(val);
	}

	public void SetJsonValueArray()
	{
		SingletonAuto<JsonPool>.instance.ReleaseJsonValue(_value);
		_value = SingletonAuto<JsonPool>.instance.CreateJsonValueArray();
	}

	public void SetJsonValueObject()
	{
		SingletonAuto<JsonPool>.instance.ReleaseJsonValue(_value);
		_value = SingletonAuto<JsonPool>.instance.CreateJsonValueObject();
	}

	private static bool WriteJson(JsonData i_json, JsonWriter writer)
	{
		if (i_json.isNone())
		{
			return writer.Write(null);
		}
		if (i_json.IsString())
		{
			return writer.Write(i_json.valueStr);
		}
		if (i_json.IsBoolean())
		{
			return writer.Write(i_json.valueBool);
		}
		if (i_json.IsDouble())
		{
			return writer.Write(i_json.valueDouble);
		}
		if (i_json.IsLong())
		{
			return writer.Write(i_json.valueLong);
		}
		if (i_json.IsArray())
		{
			if (!writer.WriteArrayStart())
			{
				return false;
			}
			if (i_json._value is JsonValueArray jsonValueArray)
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(jsonValueArray.Value); i++)
				{
					if (!WriteJson(jsonValueArray.Value[i], writer))
					{
						return false;
					}
				}
			}
			return writer.WriteArrayEnd();
		}
		if (i_json.IsObject())
		{
			if (!writer.WriteObjectStart())
			{
				return false;
			}
			if (i_json._value is JsonValueObject jsonValueObject)
			{
				foreach (KeyValuePair<string, JsonData> item in jsonValueObject)
				{
					if (!writer.WritePropertyName(item.Key))
					{
						return false;
					}
					if (!WriteJson(item.Value, writer))
					{
						return false;
					}
				}
			}
			return writer.WriteObjectEnd();
		}
		return true;
	}

	public void ReserveArrayCapacity(int addCount)
	{
		if (addCount <= 0)
		{
			return;
		}
		if (GetJsonType() != JsonType.Array)
		{
			SetJsonValueArray();
		}
		if (_value is JsonValueArray jsonValueArray)
		{
			int num = jsonValueArray.Value.Count + addCount;
			if (jsonValueArray.Value.Capacity < num)
			{
				jsonValueArray.Value.Capacity = num;
			}
		}
	}

	public void Add(JsonData data)
	{
		if (GetJsonType() != JsonType.Array)
		{
			SetJsonValueArray();
		}
		if (_value is JsonValueArray jsonValueArray)
		{
			jsonValueArray.Value.Add(data);
		}
	}

	public void Reset()
	{
		SingletonAuto<JsonPool>.instance.ReleaseJsonValue(_value);
		_value = null;
	}

	public bool ReleaseAll()
	{
		SetNone();
		return true;
	}

	public bool Equals(JsonData x)
	{
		if (x == null)
		{
			return false;
		}
		if (x.GetJsonType() != GetJsonType())
		{
			return false;
		}
		return _value.EqualsJsonValue(x._value);
	}

	public JsonType GetJsonType()
	{
		if (_value != null)
		{
			return _value.GetJsonType();
		}
		return JsonType.None;
	}

	public void SetJsonType(JsonType jsonType)
	{
		if (GetJsonType() != jsonType)
		{
			switch (jsonType)
			{
			case JsonType.None:
				SetNone();
				break;
			case JsonType.Object:
				SetJsonValueObject();
				break;
			case JsonType.Array:
				SetJsonValueArray();
				break;
			case JsonType.String:
				SetString(string.Empty);
				break;
			case JsonType.Long:
				SetLong(0L);
				break;
			case JsonType.Double:
				SetDouble(0.0);
				break;
			case JsonType.Boolean:
				SetBoolean(val: false);
				break;
			default:
				SetNone();
				break;
			}
		}
	}

	internal string internalToJson(int capacity, bool i_prettyPrint)
	{
		StringBuilder sb = ((capacity != 0) ? new StringBuilder(capacity) : new StringBuilder());
		StringWriter stringWriter = new StringWriter(sb);
		JsonWriter jsonWriter = new JsonWriter(stringWriter);
		jsonWriter.PrettyPrint = i_prettyPrint;
		jsonWriter.Validate = false;
		if (!WriteJson(this, jsonWriter))
		{
			return string.Empty;
		}
		return stringWriter.ToString();
	}

	public void ToJson(JsonWriter writer)
	{
		bool validate = writer.Validate;
		writer.Validate = false;
		WriteJson(this, writer);
		writer.Validate = validate;
	}

	public override string ToString()
	{
		if (_value is JsonValueString jsonValueString)
		{
			return jsonValueString.Value;
		}
		if (_value is JsonValueBool jsonValueBool)
		{
			return jsonValueBool.Value.ToString();
		}
		if (_value is JsonValueDouble jsonValueDouble)
		{
			return jsonValueDouble.Value.ToString();
		}
		if (_value is JsonValueLong jsonValueLong)
		{
			return jsonValueLong.Value.ToString();
		}
		if (_value is JsonValueArray)
		{
			return "JsonData Array";
		}
		if (_value is JsonValueObject)
		{
			return "JsonData Object";
		}
		return "Uninitialized JsonData";
	}

	public bool Contains(string i_prop_name)
	{
		if (!(_value is JsonValueObject jsonValueObject))
		{
			return false;
		}
		return jsonValueObject.ContainsKey(i_prop_name);
	}
}
