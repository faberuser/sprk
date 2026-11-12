using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShakmehBossTypeFormatter : IMessagePackFormatter<ShakmehBossType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShakmehBossType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ShakmehBossType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ShakmehBossType)reader.ReadInt32();
	}
}
