using System.Collections.Generic;
using LitJson;

namespace JsonBin.NBinary;

public class BinaryToJson
{
	public static uint VERSION = 539035656u;

	public static JsonData ToJson(byte[] bytes)
	{
		if (!JsonRead.CreateRoot(bytes, out var root))
		{
			return null;
		}
		return ToJson(root);
	}

	public static JsonData ToJson(JsonRead jsonRead)
	{
		switch (jsonRead.GetJsonType())
		{
		default:
			return new JsonData();
		case JsonType.Boolean:
			return new JsonData(jsonRead.GETBOOLEAN(iDefault: false));
		case JsonType.Long:
		case JsonType.ULong:
			return new JsonData(jsonRead.GETLONG(0L));
		case JsonType.Float:
			return new JsonData(jsonRead.GETFLOAT(0f));
		case JsonType.String:
			return new JsonData(jsonRead.GETSTRING(string.Empty));
		case JsonType.Array:
			return LitDeserializeArray(jsonRead);
		case JsonType.Object:
			return LitDeserializeObject(jsonRead);
		}
	}

	private static JsonData LitDeserializeArray(JsonRead jsonRead)
	{
		JsonData jsonData = new JsonData();
		for (int i = 0; i < jsonRead.Count; i++)
		{
			JsonData data = ToJson(jsonRead.GetFromArray(i));
			jsonData.Add(data);
		}
		return jsonData;
	}

	private static JsonData LitDeserializeObject(JsonRead jsonRead)
	{
		List<JsonRead.JsonReadFieldInfo> fieldInfoList = jsonRead.GetFieldInfoList();
		if (fieldInfoList == null)
		{
			fieldInfoList = jsonRead.GetFieldInfoList();
			return null;
		}
		JsonData jsonData = new JsonData();
		for (int i = 0; i < fieldInfoList.Count; i++)
		{
			JsonRead.JsonReadFieldInfo jsonReadFieldInfo = fieldInfoList[i];
			if (jsonRead.TryGetField(jsonReadFieldInfo.Key, out var value))
			{
				JsonData value2 = ToJson(value);
				jsonData.AddField(jsonReadFieldInfo.Key, value2);
			}
			else
			{
				jsonData.AddField(jsonReadFieldInfo.Key, null);
			}
		}
		return jsonData;
	}
}
