using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DamageMeterFlagFormatter : IMessagePackFormatter<DamageMeterFlag>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DamageMeterFlag value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public DamageMeterFlag Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (DamageMeterFlag)reader.ReadInt32();
	}
}
