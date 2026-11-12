using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OperationTargetSelectTypeFormatter : IMessagePackFormatter<OperationTargetSelectType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OperationTargetSelectType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public OperationTargetSelectType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (OperationTargetSelectType)reader.ReadInt32();
	}
}
