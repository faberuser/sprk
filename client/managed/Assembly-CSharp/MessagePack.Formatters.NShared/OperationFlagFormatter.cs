using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OperationFlagFormatter : IMessagePackFormatter<OperationFlag>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OperationFlag value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OperationFlag Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OperationFlag)reader.ReadInt32();
	}
}
