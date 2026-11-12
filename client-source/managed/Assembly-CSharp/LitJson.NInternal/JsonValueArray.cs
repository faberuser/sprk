using System.Collections.Generic;
using NVespa;

namespace LitJson.NInternal;

public sealed class JsonValueArray : JsonData.IJsonValue
{
	public List<JsonData> Value = new List<JsonData>();

	public JsonType GetJsonType()
	{
		return JsonType.Array;
	}

	public bool EqualsJsonValue(JsonData.IJsonValue value)
	{
		JsonValueArray jsonValueArray = value as JsonValueArray;
		if (value == null)
		{
			return false;
		}
		return Value.Equals(jsonValueArray.Value);
	}

	public void ReleaseSelf()
	{
		Clear();
		SingletonAuto<JsonPool>.instance.EnqueueJsonValue(this);
	}

	private void Clear()
	{
		for (int i = 0; i < Value.Count; i++)
		{
			JsonData jsonData = Value[i];
			if (jsonData != null)
			{
				SingletonAuto<JsonPool>.instance.ReleaseJsonData(jsonData);
			}
		}
		Value.Clear();
	}
}
