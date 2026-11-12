using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EKarmaCardTypeFormatter : IMessagePackFormatter<EKarmaCardType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EKarmaCardType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EKarmaCardType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EKarmaCardType)reader.ReadInt32();
	}
}
