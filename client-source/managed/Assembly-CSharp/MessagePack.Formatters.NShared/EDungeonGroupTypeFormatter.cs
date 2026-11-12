using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EDungeonGroupTypeFormatter : IMessagePackFormatter<EDungeonGroupType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EDungeonGroupType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EDungeonGroupType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EDungeonGroupType)reader.ReadInt32();
	}
}
