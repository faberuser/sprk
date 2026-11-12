using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TowerTypeFormatter : IMessagePackFormatter<TowerType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TowerType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public TowerType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (TowerType)reader.ReadInt32();
	}
}
