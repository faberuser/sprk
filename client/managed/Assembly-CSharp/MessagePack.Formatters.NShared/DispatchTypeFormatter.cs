using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DispatchTypeFormatter : IMessagePackFormatter<DispatchType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DispatchType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public DispatchType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (DispatchType)reader.ReadInt32();
	}
}
