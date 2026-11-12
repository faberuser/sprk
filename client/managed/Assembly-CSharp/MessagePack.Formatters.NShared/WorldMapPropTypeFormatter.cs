using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldMapPropTypeFormatter : IMessagePackFormatter<WorldMapPropType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldMapPropType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public WorldMapPropType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (WorldMapPropType)reader.ReadInt32();
	}
}
