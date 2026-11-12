using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ResetTypeFormatter : IMessagePackFormatter<ResetType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ResetType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ResetType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ResetType)reader.ReadInt32();
	}
}
