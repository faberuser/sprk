using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WinDeciderTypeFormatter : IMessagePackFormatter<WinDeciderType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WinDeciderType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public WinDeciderType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (WinDeciderType)reader.ReadInt32();
	}
}
