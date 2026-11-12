using System;
using System.Collections.Generic;
using JsonBin.NBinary;
using JsonBin.NBinary.NRead;
using LitJson;
using UnityEngine;

namespace JsonBin;

public struct JsonRead
{
	public class JsonReadArrayInfo
	{
		public int IndexInArray;

		public int ValueLength;

		public JsonRead ValueJsonRead;
	}

	public class JsonReadFieldInfo
	{
		public string Key;

		public int ValueLength;

		public JsonRead ValueJsonRead;
	}

	private readonly JsonData _data;

	private readonly JsonBinSpan _span;

	private readonly KeyContainer _keyContainer;

	public bool IsValid
	{
		get
		{
			if (_data != null)
			{
				return true;
			}
			return _span.IsValid;
		}
	}

	public JsonRead this[string key] => GetField(key);

	public int Count => GetCount();

	public JsonRead GetJsonAny(string key1, string key2)
	{
		if (TryGetField(key1, out var value))
		{
			return value;
		}
		if (TryGetField(key2, out value))
		{
			return value;
		}
		return default(JsonRead);
	}

	public Vector2 GETVECTOR2(Vector2 defaultValue)
	{
		if (!TryGetVector2(out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public bool TryGetVector2(out Vector2 value)
	{
		if (!IsObject())
		{
			value = Vector2.zero;
			return false;
		}
		value = new Vector2(this["x"].GETFLOAT(0f), this["y"].GETFLOAT(0f));
		return true;
	}

	public Vector3 GETVECTOR3(Vector3 defaultValue)
	{
		if (!TryGetVector3(out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public bool TryGetVector3(out Vector3 value)
	{
		if (!IsObject())
		{
			value = Vector3.zero;
			return false;
		}
		value = new Vector3(this["x"].GETFLOAT(0f), this["y"].GETFLOAT(0f), this["z"].GETFLOAT(0f));
		return true;
	}

	public Quaternion GETQUATERNION(Quaternion defaultValue)
	{
		if (!TryGetQuaternion(out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public bool TryGetQuaternion(out Quaternion value)
	{
		if (!IsObject())
		{
			value = Quaternion.identity;
			return false;
		}
		value = new Quaternion(this["x"].GETFLOAT(0f), this["y"].GETFLOAT(0f), this["z"].GETFLOAT(0f), this["w"].GETFLOAT(0f));
		return true;
	}

	public Rect GETRECT(Rect defaultValue)
	{
		if (!TryGetRect(out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public bool TryGetRect(out Rect value)
	{
		if (!IsObject())
		{
			value = default(Rect);
			return false;
		}
		if (!this["pos"].TryGetVector2(out var value2) || !this["size"].TryGetVector2(out var value3))
		{
			value = default(Rect);
			return false;
		}
		value = new Rect(value2, value3);
		return true;
	}

	public AnimationCurve GETANIMATIONCURVE(AnimationCurve defaultValue)
	{
		return GetAnimationCurve(defaultValue);
	}

	public AnimationCurve GetAnimationCurve(AnimationCurve defaultValue)
	{
		if (!TryGetAnimationCurve(out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public bool TryGetAnimationCurve(out AnimationCurve value)
	{
		value = AnimationCurveBuilder.LoadFromJson(this);
		return value != null;
	}

	public Color GETCOLOR(Color defaultValue)
	{
		return GetColor(defaultValue);
	}

	public Color GetColor(Color defaultValue)
	{
		if (!TryGetColor(out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public bool TryGetColor(out Color value)
	{
		if (!IsObject())
		{
			value = Color.white;
			return false;
		}
		value = new Color(this["r"].GETFLOAT(1f), this["g"].GETFLOAT(1f), this["b"].GETFLOAT(1f), this["a"].GETFLOAT(1f));
		return true;
	}

	public Gradient GETGRADIENT(Gradient defaultValue)
	{
		return GetGradient(defaultValue);
	}

	public Gradient GetGradient(Gradient defaultValue)
	{
		if (!TryGetGradient(out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public bool TryGetGradient(out Gradient value)
	{
		if (!IsObject())
		{
			value = null;
			return false;
		}
		JsonRead jsonRead = this["colorkeys_array"];
		JsonRead jsonRead2 = this["alphakeys_array"];
		if (!jsonRead.IsArray() || !jsonRead2.IsArray())
		{
			value = null;
			return false;
		}
		GradientColorKey[] array = new GradientColorKey[jsonRead.Count];
		for (int i = 0; i < jsonRead.Count; i++)
		{
			JsonRead fromArray = jsonRead.GetFromArray(i);
			array[i] = new GradientColorKey
			{
				color = fromArray["color"].GetColor(Color.white),
				time = fromArray["time"].GETFLOAT(0f)
			};
		}
		GradientAlphaKey[] array2 = new GradientAlphaKey[jsonRead2.Count];
		for (int j = 0; j < jsonRead2.Count; j++)
		{
			JsonRead fromArray2 = jsonRead2.GetFromArray(j);
			array2[j] = new GradientAlphaKey
			{
				alpha = fromArray2["alpha"].GETFLOAT(1f),
				time = fromArray2["time"].GETFLOAT(0f)
			};
		}
		Gradient gradient = new Gradient();
		gradient.SetKeys(array, array2);
		value = gradient;
		return true;
	}

	public static bool CreateRoot(byte[] bytes, out JsonRead root)
	{
		KeyContainer keyContainer;
		return CreateRoot(bytes, out root, out keyContainer);
	}

	public static bool CreateRoot(byte[] bytes, out JsonRead root, out KeyContainer keyContainer)
	{
		JsonBinReader jsonBinReader = new JsonBinReader(bytes);
		if (jsonBinReader.ReadPacked_UInt() != BinaryToJson.VERSION)
		{
			root = default(JsonRead);
			keyContainer = null;
			return false;
		}
		JsonBinReader reader = jsonBinReader.Slice(0);
		int num = reader.ReadPacked_Int();
		JsonBinReader jsonBinReader2 = reader.Slice(num);
		jsonBinReader2.ReadPacked_Int();
		keyContainer = new KeyContainer();
		if (!keyContainer.Deserialize(reader, num))
		{
			root = default(JsonRead);
			return false;
		}
		root = new JsonRead(keyContainer, jsonBinReader2.Span(0));
		return true;
	}

	public bool IsNone()
	{
		return GetJsonType() == JsonType.None;
	}

	public bool IsArray()
	{
		return GetJsonType() == JsonType.Array;
	}

	public bool IsObject()
	{
		return GetJsonType() == JsonType.Object;
	}

	public bool IsBoolean()
	{
		return GetJsonType() == JsonType.Boolean;
	}

	public bool IsString()
	{
		return GetJsonType() == JsonType.String;
	}

	public JsonRead(JsonData data)
	{
		_data = data;
		_span = default(JsonBinSpan);
		_keyContainer = null;
	}

	public JsonRead(KeyContainer keyContainer, JsonBinSpan span)
	{
		_data = null;
		_span = span;
		_keyContainer = keyContainer;
	}

	public JsonType GetJsonType()
	{
		if (_data != null)
		{
			return _data.GetJsonType() switch
			{
				LitJson.JsonType.None => JsonType.None, 
				LitJson.JsonType.Object => JsonType.Object, 
				LitJson.JsonType.Array => JsonType.Array, 
				LitJson.JsonType.String => JsonType.String, 
				LitJson.JsonType.Long => JsonType.Long, 
				LitJson.JsonType.Double => JsonType.Double, 
				LitJson.JsonType.Boolean => JsonType.Boolean, 
				_ => JsonType.None, 
			};
		}
		return _span.NextJsonType;
	}

	public bool Contains(string key)
	{
		if (TryGetField(key, out var _))
		{
			return true;
		}
		return false;
	}

	public string DumpShort()
	{
		string text = $"Type={GetJsonType()}";
		switch (GetJsonType())
		{
		case JsonType.Boolean:
			text = text + ", Value=" + GETBOOLEAN(iDefault: false);
			break;
		case JsonType.Long:
		case JsonType.ULong:
			text = text + ", Value=" + GETLONG(0L);
			break;
		case JsonType.Float:
			text = text + ", Value=" + GETFLOAT(0f);
			break;
		case JsonType.String:
			text = text + ", Value=" + GETSTRING(string.Empty);
			break;
		}
		return text;
	}

	public int GetCount()
	{
		if (_data != null)
		{
			return _data.Count;
		}
		if (!_span.TryRead_Native(out byte value, 0))
		{
			return 0;
		}
		switch (value)
		{
		case 215:
		{
			if (!_span.TryRead_Native(out byte value6, 1))
			{
				return 0;
			}
			return value6;
		}
		case 216:
		{
			if (!_span.TryRead_Native(out ushort value4, 1))
			{
				return 0;
			}
			return value4;
		}
		case 217:
		{
			if (!_span.TryRead_Native(out int value7, 1))
			{
				return 0;
			}
			return value7;
		}
		case 218:
		{
			if (!_span.TryRead_Native(out byte value3, 1))
			{
				return 0;
			}
			return value3;
		}
		case 219:
		{
			if (!_span.TryRead_Native(out ushort value5, 1))
			{
				return 0;
			}
			return value5;
		}
		case 220:
		{
			if (!_span.TryRead_Native(out int value2, 1))
			{
				return 0;
			}
			return value2;
		}
		default:
			return 0;
		}
	}

	public bool GETBOOLEAN(bool iDefault)
	{
		if (!TryGetBoolean(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public int GETINT(int iDefault)
	{
		if (!TryGetInt(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public long GETLONG(long iDefault)
	{
		if (!TryGetLong(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public ulong GETULONG(ulong iDefault)
	{
		if (!TryGetULong(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public float GETFLOAT(float iDefault)
	{
		if (!TryGetFloat(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public string GETSTRING(string iDefault)
	{
		if (!TryGetString(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public EnumType GETENUM<EnumType>(EnumType i_default)
	{
		if (!TryGetEnum<EnumType>(out var value))
		{
			return i_default;
		}
		return value;
	}

	public bool GetBoolean(bool iDefault)
	{
		if (!TryGetBoolean(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public sbyte GetSByte(sbyte iDefault)
	{
		if (!TryGetSByte(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public short GetShort(short iDefault)
	{
		if (!TryGetShort(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public int GetInt(int iDefault)
	{
		if (!TryGetInt(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public long GetLong(long iDefault)
	{
		if (!TryGetLong(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public byte GetByte(byte iDefault)
	{
		if (!TryGetByte(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public ushort GetUShort(ushort iDefault)
	{
		if (!TryGetUShort(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public uint GetUInt(uint iDefault)
	{
		if (!TryGetUInt(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public ulong GetULong(ulong iDefault)
	{
		if (!TryGetULong(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public float GetFloat(float iDefault)
	{
		if (!TryGetFloat(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public string GetString(string iDefault)
	{
		if (!TryGetString(out var value))
		{
			return iDefault;
		}
		return value;
	}

	public JsonRead GetFromArray(int index)
	{
		if (!TryGetFromArray(index, out var value))
		{
			return default(JsonRead);
		}
		return value;
	}

	public JsonRead GetField(string key)
	{
		if (!TryGetField(key, out var value))
		{
			return default(JsonRead);
		}
		return value;
	}

	public bool TryGetBoolean(out bool value)
	{
		if (_data != null)
		{
			return _data.TryGetBoolean(out value);
		}
		if (_span.TryReadPacked_Boolean(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return bool.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetSByte(out sbyte value)
	{
		if (_data != null)
		{
			int o_value;
			bool result = _data.TryGetInt(out o_value);
			value = (sbyte)o_value;
			return result;
		}
		if (_span.TryReadPacked_SByte(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return sbyte.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetShort(out short value)
	{
		if (_data != null)
		{
			int o_value;
			bool result = _data.TryGetInt(out o_value);
			value = (short)o_value;
			return result;
		}
		if (_span.TryReadPacked_Short(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return short.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetInt(out int value)
	{
		if (_data != null)
		{
			return _data.TryGetInt(out value);
		}
		if (_span.TryReadPacked_Int(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return int.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetLong(out long value)
	{
		if (_data != null)
		{
			return _data.TryGetLong(out value);
		}
		if (_span.TryReadPacked_Long(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return long.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetByte(out byte value)
	{
		if (_data != null)
		{
			int o_value;
			bool result = _data.TryGetInt(out o_value);
			value = (byte)o_value;
			return result;
		}
		if (_span.TryReadPacked_Byte(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return byte.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetUShort(out ushort value)
	{
		if (_data != null)
		{
			int o_value;
			bool result = _data.TryGetInt(out o_value);
			value = (ushort)o_value;
			return result;
		}
		if (_span.TryReadPacked_UShort(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return ushort.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetUInt(out uint value)
	{
		if (_data != null)
		{
			int o_value;
			bool result = _data.TryGetInt(out o_value);
			value = (uint)o_value;
			return result;
		}
		if (_span.TryReadPacked_UInt(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return uint.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetULong(out ulong value)
	{
		if (_data != null)
		{
			long o_value;
			bool result = _data.TryGetLong(out o_value);
			value = (ulong)o_value;
			return result;
		}
		if (_span.TryReadPacked_ULong(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return ulong.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetFloat(out float value)
	{
		if (_data != null)
		{
			return _data.TryGetFloat(out value);
		}
		if (_span.TryReadPacked_Float(out value))
		{
			return true;
		}
		if (TryGetString(out var value2))
		{
			return float.TryParse(value2, out value);
		}
		return false;
	}

	public bool TryGetString(out string value)
	{
		if (_data != null)
		{
			return _data.TryGetString(out value);
		}
		if (_span.TryReadPacked_IndexedStr(out var stringIndex))
		{
			value = _keyContainer.GetValueString(stringIndex);
			return true;
		}
		return _span.TryReadPacked_String(out value);
	}

	public bool TryGetEnum<EnumType>(out EnumType value)
	{
		if (_data != null)
		{
			return _data.TryGetEnum<EnumType>(out value);
		}
		if (_span.TryReadPacked_IndexedStr(out var stringIndex) && _keyContainer.TryGetEnumFromIndexedStr<EnumType>(stringIndex, out value))
		{
			return true;
		}
		if (!_span.TryReadPacked_String(out var value2))
		{
			value = default(EnumType);
			return false;
		}
		return Vespa_String_Extension_Method.TryToEnum<EnumType>(value2, out value);
	}

	public bool TryGetFromArray(int index, out JsonRead value)
	{
		if (_data != null)
		{
			JsonData fromArray = _data.GetFromArray(index);
			if (fromArray == null)
			{
				value = default(JsonRead);
				return false;
			}
			value = new JsonRead(fromArray);
			return true;
		}
		if (index < 0)
		{
			value = default(JsonRead);
			return false;
		}
		if (!_span.TryRead_Native(out byte value2, 0))
		{
			value = default(JsonRead);
			return false;
		}
		switch (value2)
		{
		case 215:
		{
			if (!_span.TryRead_Native(out byte value7, 1))
			{
				value = default(JsonRead);
				return false;
			}
			if (index >= value7)
			{
				value = default(JsonRead);
				return false;
			}
			int num5 = 2;
			if (!_span.TryRead_Native(out byte value8, num5 + index))
			{
				value = default(JsonRead);
				return false;
			}
			int num6 = value7;
			value = new JsonRead(_keyContainer, _span.Slice(num5 + num6 + value8));
			return true;
		}
		case 216:
		{
			if (!_span.TryRead_Native(out ushort value5, 1))
			{
				value = default(JsonRead);
				return false;
			}
			if (index >= value5)
			{
				value = default(JsonRead);
				return false;
			}
			int num3 = 3;
			if (!_span.TryRead_Native(out ushort value6, num3 + index * 2))
			{
				value = default(JsonRead);
				return false;
			}
			int num4 = value5 * 2;
			value = new JsonRead(_keyContainer, _span.Slice(num3 + num4 + value6));
			return true;
		}
		case 217:
		{
			if (!_span.TryRead_Native(out int value3, 1))
			{
				value = default(JsonRead);
				return false;
			}
			if (index >= value3)
			{
				value = default(JsonRead);
				return false;
			}
			int num = 5;
			if (!_span.TryRead_Native(out int value4, num + index * 4))
			{
				value = default(JsonRead);
				return false;
			}
			int num2 = value3 * 4;
			value = new JsonRead(_keyContainer, _span.Slice(num + num2 + value4));
			return true;
		}
		default:
			value = default(JsonRead);
			return false;
		}
	}

	public bool TryGetField(string key, out JsonRead value)
	{
		if (_data != null)
		{
			JsonData jsonData = _data[key];
			if (jsonData == null)
			{
				value = default(JsonRead);
				return false;
			}
			value = new JsonRead(jsonData);
			return true;
		}
		if (!_span.TryRead_Native(out byte value2, 0))
		{
			value = default(JsonRead);
			return false;
		}
		if (JsonBinCode.ToJsonType(value2) != JsonType.Object)
		{
			value = default(JsonRead);
			return false;
		}
		if (String_Extension_Method.IsNullOrEmpty(key))
		{
			value = default(JsonRead);
			return false;
		}
		int hash = _keyContainer.GetHash(key);
		if (hash == 0)
		{
			value = default(JsonRead);
			return false;
		}
		int index;
		switch (value2)
		{
		case 218:
		{
			if (!_span.TryRead_Native(out byte value5, 1))
			{
				value = default(JsonRead);
				return false;
			}
			int num4 = hash % value5;
			if (num4 < 0)
			{
				num4 += value5;
			}
			int num5 = 2;
			if (!_span.TryRead_Native(out byte value6, num5 + num4))
			{
				value = default(JsonRead);
				return false;
			}
			int num6 = value5;
			index = num5 + num6 + value6;
			break;
		}
		case 219:
		{
			if (!_span.TryRead_Native(out ushort value7, 1))
			{
				value = default(JsonRead);
				return false;
			}
			int num7 = hash % value7;
			if (num7 < 0)
			{
				num7 += value7;
			}
			int num8 = 3;
			if (!_span.TryRead_Native(out ushort value8, num8 + num7 * 2))
			{
				value = default(JsonRead);
				return false;
			}
			int num9 = value7 * 2;
			index = num8 + num9 + value8;
			break;
		}
		case 220:
		{
			if (!_span.TryRead_Native(out int value3, 1))
			{
				value = default(JsonRead);
				return false;
			}
			int num = hash % value3;
			if (num < 0)
			{
				num += value3;
			}
			int num2 = 5;
			if (!_span.TryRead_Native(out int value4, num2 + num * 4))
			{
				value = default(JsonRead);
				return false;
			}
			int num3 = value3 * 4;
			index = num2 + num3 + value4;
			break;
		}
		default:
			value = default(JsonRead);
			return false;
		}
		if (!_span.Slice(index).TryReadPacked_Int(out var value9, out var nextSpan))
		{
			value = default(JsonRead);
			return false;
		}
		for (int i = 0; i < value9; i++)
		{
			if (!nextSpan.TryReadPacked_Int(out var value10, out nextSpan))
			{
				value = default(JsonRead);
				return false;
			}
			if (!nextSpan.TryReadPacked_Int(out var value11, out nextSpan))
			{
				value = default(JsonRead);
				return false;
			}
			if (hash != value10)
			{
				nextSpan = nextSpan.Slice(value11);
				continue;
			}
			value = new JsonRead(_keyContainer, nextSpan);
			return true;
		}
		value = default(JsonRead);
		return false;
	}

	public List<JsonReadArrayInfo> GetArrayInfoList(int totalSize = 0)
	{
		if (_data != null)
		{
			List<JsonReadArrayInfo> list = new List<JsonReadArrayInfo>();
			for (int i = 0; i < _data.Count; i++)
			{
				list.Add(new JsonReadArrayInfo
				{
					IndexInArray = i,
					ValueJsonRead = new JsonRead(_data.GetFromArray(i))
				});
			}
			return list;
		}
		if (!_span.TryRead_Native(out byte value, 0))
		{
			return null;
		}
		List<int> list2 = new List<int>();
		switch (value)
		{
		case 215:
		{
			if (!_span.TryRead_Native(out byte value4, 1))
			{
				return null;
			}
			int num3 = 2;
			for (int k = 0; k < value4; k++)
			{
				if (!_span.TryRead_Native(out byte value5, num3 + k))
				{
					return null;
				}
				int num4 = value4;
				int item2 = num3 + num4 + value5;
				list2.Add(item2);
			}
			break;
		}
		case 216:
		{
			if (!_span.TryRead_Native(out ushort value6, 1))
			{
				return null;
			}
			int num5 = 3;
			for (int l = 0; l < value6; l++)
			{
				if (!_span.TryRead_Native(out ushort value7, num5 + l * 2))
				{
					return null;
				}
				int num6 = value6 * 2;
				int item3 = num5 + num6 + value7;
				list2.Add(item3);
			}
			break;
		}
		case 217:
		{
			if (!_span.TryRead_Native(out int value2, 1))
			{
				return null;
			}
			int num = 5;
			for (int j = 0; j < value2; j++)
			{
				if (!_span.TryRead_Native(out int value3, num + j * 4))
				{
					return null;
				}
				int num2 = value2 * 4;
				int item = num + num2 + value3;
				list2.Add(item);
			}
			break;
		}
		default:
			return null;
		}
		List<JsonReadArrayInfo> list3 = new List<JsonReadArrayInfo>();
		for (int m = 0; m < list2.Count; m++)
		{
			int num7 = list2[m];
			JsonRead valueJsonRead = new JsonRead(_keyContainer, _span.Slice(num7));
			int valueLength = ((m >= list2.Count - 1) ? (totalSize - num7) : (list2[m + 1] - num7));
			list3.Add(new JsonReadArrayInfo
			{
				IndexInArray = m,
				ValueLength = valueLength,
				ValueJsonRead = valueJsonRead
			});
		}
		return list3;
	}

	public List<JsonReadFieldInfo> GetFieldInfoList(string keyFilter = null)
	{
		if (_data != null)
		{
			List<JsonReadFieldInfo> list = new List<JsonReadFieldInfo>();
			{
				foreach (KeyValuePair<string, JsonData> item4 in _data.ToDictionary())
				{
					if (string.IsNullOrEmpty(keyFilter) || item4.Key.IndexOf(keyFilter, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						list.Add(new JsonReadFieldInfo
						{
							Key = item4.Key,
							ValueJsonRead = new JsonRead(item4.Value)
						});
					}
				}
				return list;
			}
		}
		if (!_span.TryRead_Native(out byte value, 0))
		{
			return null;
		}
		List<int> list2 = new List<int>();
		switch (value)
		{
		case 218:
		{
			if (!_span.TryRead_Native(out byte value4, 1))
			{
				return null;
			}
			for (int j = 0; j < value4; j++)
			{
				int num3 = 2;
				if (!_span.TryRead_Native(out byte value5, num3 + j))
				{
					return null;
				}
				int num4 = value4;
				int item2 = num3 + num4 + value5;
				list2.Add(item2);
			}
			break;
		}
		case 219:
		{
			if (!_span.TryRead_Native(out ushort value6, 1))
			{
				return null;
			}
			for (int k = 0; k < value6; k++)
			{
				int num5 = 3;
				if (!_span.TryRead_Native(out ushort value7, num5 + k * 2))
				{
					return null;
				}
				int num6 = value6 * 2;
				int item3 = num5 + num6 + value7;
				list2.Add(item3);
			}
			break;
		}
		case 220:
		{
			if (!_span.TryRead_Native(out int value2, 1))
			{
				return null;
			}
			for (int i = 0; i < value2; i++)
			{
				int num = 5;
				if (!_span.TryRead_Native(out int value3, num + i * 4))
				{
					return null;
				}
				int num2 = value2 * 4;
				int item = num + num2 + value3;
				list2.Add(item);
			}
			break;
		}
		default:
			return null;
		}
		List<JsonReadFieldInfo> list3 = new List<JsonReadFieldInfo>();
		for (int l = 0; l < list2.Count; l++)
		{
			int index = list2[l];
			if (!_span.Slice(index).TryReadPacked_Int(out var value8, out var nextSpan))
			{
				return null;
			}
			for (int m = 0; m < value8; m++)
			{
				if (!nextSpan.TryReadPacked_Int(out var value9, out nextSpan))
				{
					return null;
				}
				string key = _keyContainer.GetKey(value9);
				if (!nextSpan.TryReadPacked_Int(out var value10, out nextSpan))
				{
					return null;
				}
				bool flag = false;
				if (!string.IsNullOrEmpty(keyFilter) && key.IndexOf(keyFilter, StringComparison.OrdinalIgnoreCase) < 0)
				{
					flag = true;
				}
				if (!flag)
				{
					JsonReadFieldInfo jsonReadFieldInfo = new JsonReadFieldInfo();
					jsonReadFieldInfo.Key = key;
					jsonReadFieldInfo.ValueLength = value10;
					jsonReadFieldInfo.ValueJsonRead = new JsonRead(_keyContainer, nextSpan);
					list3.Add(jsonReadFieldInfo);
				}
				nextSpan = nextSpan.Slice(value10);
			}
		}
		return list3;
	}
}
