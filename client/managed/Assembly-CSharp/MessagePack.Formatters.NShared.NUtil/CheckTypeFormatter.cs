using NShared.NUtil;

namespace MessagePack.Formatters.NShared.NUtil;

public sealed class CheckTypeFormatter : IMessagePackFormatter<CheckType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CheckType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CheckType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CheckType)reader.ReadInt32();
	}
}
