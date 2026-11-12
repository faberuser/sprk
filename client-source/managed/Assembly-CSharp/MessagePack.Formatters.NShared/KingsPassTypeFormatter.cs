using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KingsPassTypeFormatter : IMessagePackFormatter<KingsPassType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KingsPassType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public KingsPassType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (KingsPassType)reader.ReadInt32();
	}
}
