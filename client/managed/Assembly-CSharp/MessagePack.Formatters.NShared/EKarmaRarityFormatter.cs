using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EKarmaRarityFormatter : IMessagePackFormatter<EKarmaRarity>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EKarmaRarity value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EKarmaRarity Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EKarmaRarity)reader.ReadInt32();
	}
}
