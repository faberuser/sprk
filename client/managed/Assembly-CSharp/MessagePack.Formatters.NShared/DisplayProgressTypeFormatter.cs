using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DisplayProgressTypeFormatter : IMessagePackFormatter<DisplayProgressType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DisplayProgressType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public DisplayProgressType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (DisplayProgressType)reader.ReadInt32();
	}
}
