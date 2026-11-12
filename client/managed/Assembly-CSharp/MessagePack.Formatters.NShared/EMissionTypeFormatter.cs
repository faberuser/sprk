using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EMissionTypeFormatter : IMessagePackFormatter<EMissionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EMissionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EMissionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EMissionType)reader.ReadInt32();
	}
}
