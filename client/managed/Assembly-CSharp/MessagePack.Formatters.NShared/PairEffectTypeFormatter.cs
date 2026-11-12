using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PairEffectTypeFormatter : IMessagePackFormatter<PairEffectType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PairEffectType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PairEffectType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PairEffectType)reader.ReadInt32();
	}
}
