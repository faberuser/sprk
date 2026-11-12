using System;
using NVespa;

namespace LitJson;

public class JsonMapper
{
	public static void ToObjectAsync(string stringJson, Action<JsonData> onComplete)
	{
		JsonData json = null;
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			json = ToObject(stringJson);
		}, (Action)delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, json);
		}, ThreadManager.EEntryType.LitJson);
	}

	public static JsonData ToObject(string json)
	{
		if (String_Extension_Method.IsNullOrEmpty(json))
		{
			return null;
		}
		Tuple<JsonData, bool> tuple = ReadValue(new JsonReader(json));
		if (!tuple.Item2)
		{
			return null;
		}
		return tuple.Item1;
	}

	public static void DebugLog(string i_log)
	{
	}

	private static Tuple<JsonData, bool> ReadValue(JsonReader reader)
	{
		if (reader.Read() == ExceptionBool.Exception)
		{
			return Tuple.Create<JsonData, bool>(null, i_item2: false);
		}
		if (reader.Token == JsonToken.ArrayEnd)
		{
			DebugLog("Token is ArrayEnd(return null)");
			return Tuple.Create<JsonData, bool>(null, i_item2: true);
		}
		if (reader.Token == JsonToken.Null)
		{
			DebugLog("Token is null");
			return Tuple.Create<JsonData, bool>(null, i_item2: true);
		}
		JsonData jsonData = new JsonData();
		if (reader.Token == JsonToken.String)
		{
			DebugLog("Token is String: " + (string)reader.Value);
			jsonData.SetString((string)reader.Value);
			return Tuple.Create(jsonData, i_item2: true);
		}
		if (reader.Token == JsonToken.Double)
		{
			DebugLog("Token is Double: " + (double)reader.Value);
			jsonData.SetDouble((double)reader.Value);
			return Tuple.Create(jsonData, i_item2: true);
		}
		if (reader.Token == JsonToken.Long)
		{
			DebugLog("Token is Long: " + (long)reader.Value);
			jsonData.SetLong((long)reader.Value);
			return Tuple.Create(jsonData, i_item2: true);
		}
		if (reader.Token == JsonToken.Boolean)
		{
			DebugLog("Token is Boolean" + (bool)reader.Value);
			jsonData.SetBoolean((bool)reader.Value);
			return Tuple.Create(jsonData, i_item2: true);
		}
		if (reader.Token == JsonToken.ArrayStart)
		{
			DebugLog("Token is ArrayStart");
			jsonData.SetJsonType(JsonType.Array);
			while (true)
			{
				Tuple<JsonData, bool> tuple = ReadValue(reader);
				if (!tuple.Item2)
				{
					return tuple;
				}
				JsonData item = tuple.Item1;
				if (item == null && reader.Token == JsonToken.ArrayEnd)
				{
					break;
				}
				jsonData.Add(item);
			}
			DebugLog("Token is ArrayEnd");
		}
		else if (reader.Token == JsonToken.ObjectStart)
		{
			DebugLog("Token is ObjectStart");
			jsonData.SetJsonType(JsonType.Object);
			while (true)
			{
				if (reader.Read() == ExceptionBool.Exception)
				{
					return Tuple.Create(jsonData, i_item2: false);
				}
				if (reader.Token == JsonToken.ObjectEnd)
				{
					break;
				}
				string text = (string)reader.Value;
				DebugLog("property is " + text);
				Tuple<JsonData, bool> tuple2 = ReadValue(reader);
				if (!tuple2.Item2)
				{
					return tuple2;
				}
				JsonData item2 = tuple2.Item1;
				jsonData.AddField(text, item2);
			}
			DebugLog("Token is ObjectEnd");
		}
		return Tuple.Create(jsonData, i_item2: true);
	}
}
