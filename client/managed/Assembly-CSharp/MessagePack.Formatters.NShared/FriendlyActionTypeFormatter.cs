using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class FriendlyActionTypeFormatter : IMessagePackFormatter<FriendlyActionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, FriendlyActionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public FriendlyActionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (FriendlyActionType)reader.ReadInt32();
	}
}
