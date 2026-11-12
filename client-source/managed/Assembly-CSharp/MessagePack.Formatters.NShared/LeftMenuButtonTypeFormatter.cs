using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LeftMenuButtonTypeFormatter : IMessagePackFormatter<LeftMenuButtonType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LeftMenuButtonType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public LeftMenuButtonType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (LeftMenuButtonType)reader.ReadInt32();
	}
}
