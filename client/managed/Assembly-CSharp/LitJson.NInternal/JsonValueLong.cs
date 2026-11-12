using NVespa;

namespace LitJson.NInternal;

public sealed class JsonValueLong : JsonData.IJsonValue
{
	public long Value;

	public JsonType GetJsonType()
	{
		return JsonType.Long;
	}

	public bool EqualsJsonValue(JsonData.IJsonValue value)
	{
		JsonValueLong jsonValueLong = value as JsonValueLong;
		if (value == null)
		{
			return false;
		}
		return Value.Equals(jsonValueLong.Value);
	}

	public void ReleaseSelf()
	{
		Value = 0L;
		SingletonAuto<JsonPool>.instance.EnqueueJsonValue(this);
	}
}
