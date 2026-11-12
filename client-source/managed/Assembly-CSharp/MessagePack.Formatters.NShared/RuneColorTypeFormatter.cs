using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RuneColorTypeFormatter : IMessagePackFormatter<RuneColorType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RuneColorType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RuneColorType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RuneColorType)reader.ReadInt32();
	}
}
