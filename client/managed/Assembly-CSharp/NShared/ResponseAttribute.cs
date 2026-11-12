namespace NShared;

public class ResponseAttribute : JsonSerializableAttribute
{
	public ResponseAttribute()
	{
		base.Serialize = false;
		base.Deserialize = true;
		base.Javascript = true;
	}
}
