using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DelayedSpawnTypeFormatter : IMessagePackFormatter<DelayedSpawnType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DelayedSpawnType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public DelayedSpawnType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (DelayedSpawnType)reader.ReadInt32();
	}
}
