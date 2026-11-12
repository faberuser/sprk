using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EGuildFlagDisplayTypeFormatter : IMessagePackFormatter<EGuildFlagDisplayType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EGuildFlagDisplayType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EGuildFlagDisplayType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EGuildFlagDisplayType)reader.ReadInt32();
	}
}
