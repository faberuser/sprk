using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PatchSpecTypeFormatter : IMessagePackFormatter<PatchSpecType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PatchSpecType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PatchSpecType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PatchSpecType)reader.ReadInt32();
	}
}
