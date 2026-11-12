using NVespa;

namespace LitJson.NInternal;

public sealed class JsonValueDouble : JsonData.IJsonValue
{
	public double Value;

	public JsonType GetJsonType()
	{
		return JsonType.Double;
	}

	public bool EqualsJsonValue(JsonData.IJsonValue value)
	{
		JsonValueDouble jsonValueDouble = value as JsonValueDouble;
		if (value == null)
		{
			return false;
		}
		return Value.Equals(jsonValueDouble.Value);
	}

	public void ReleaseSelf()
	{
		Value = 0.0;
		SingletonAuto<JsonPool>.instance.EnqueueJsonValue(this);
	}
}
