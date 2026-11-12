using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CcMotionTypeFormatter : IMessagePackFormatter<CcMotionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CcMotionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CcMotionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CcMotionType)reader.ReadInt32();
	}
}
