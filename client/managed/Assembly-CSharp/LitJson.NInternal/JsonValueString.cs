using NVespa;

namespace LitJson.NInternal;

public sealed class JsonValueString : JsonData.IJsonValue
{
	public string Value = string.Empty;

	public JsonType GetJsonType()
	{
		return JsonType.String;
	}

	public bool EqualsJsonValue(JsonData.IJsonValue value)
	{
		JsonValueString jsonValueString = value as JsonValueString;
		if (value == null)
		{
			return false;
		}
		return Value.Equals(jsonValueString.Value);
	}

	public void ReleaseSelf()
	{
		Value = string.Empty;
		SingletonAuto<JsonPool>.instance.EnqueueJsonValue(this);
	}
}
