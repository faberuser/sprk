namespace NShared;

public class MessagePacketAttribute : JsonSerializableAttribute
{
	public MessagePacketAttribute()
	{
		base.Serialize = true;
		base.Deserialize = true;
		base.Javascript = true;
	}
}
