using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureTransformTypeFormatter : IMessagePackFormatter<CreatureTransformType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureTransformType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CreatureTransformType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CreatureTransformType)reader.ReadInt32();
	}
}
