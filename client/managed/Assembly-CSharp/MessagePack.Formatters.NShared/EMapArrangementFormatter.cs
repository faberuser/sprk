using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EMapArrangementFormatter : IMessagePackFormatter<EMapArrangement>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EMapArrangement value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EMapArrangement Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EMapArrangement)reader.ReadInt32();
	}
}
