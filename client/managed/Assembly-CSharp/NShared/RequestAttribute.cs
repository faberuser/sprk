namespace NShared;

public class RequestAttribute : JsonSerializableAttribute
{
	public RequestAttribute()
	{
		base.Serialize = true;
		base.Deserialize = true;
		base.Javascript = true;
	}
}
