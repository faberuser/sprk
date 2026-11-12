using NVespa;

namespace LitJson.NInternal;

public sealed class JsonValueBool : JsonData.IJsonValue
{
	public bool Value;

	public JsonType GetJsonType()
	{
		return JsonType.Boolean;
	}

	public bool EqualsJsonValue(JsonData.IJsonValue value)
	{
		JsonValueBool jsonValueBool = value as JsonValueBool;
		if (value == null)
		{
			return false;
		}
		return Value.Equals(jsonValueBool.Value);
	}

	public void ReleaseSelf()
	{
		Value = false;
		SingletonAuto<JsonPool>.instance.EnqueueJsonValue(this);
	}
}
