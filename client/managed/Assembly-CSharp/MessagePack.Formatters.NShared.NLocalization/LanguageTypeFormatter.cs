using NShared.NLocalization;

namespace MessagePack.Formatters.NShared.NLocalization;

public sealed class LanguageTypeFormatter : IMessagePackFormatter<LanguageType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LanguageType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public LanguageType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (LanguageType)reader.ReadInt32();
	}
}
