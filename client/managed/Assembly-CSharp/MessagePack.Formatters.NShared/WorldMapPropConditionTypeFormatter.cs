using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldMapPropConditionTypeFormatter : IMessagePackFormatter<WorldMapPropConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldMapPropConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public WorldMapPropConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (WorldMapPropConditionType)reader.ReadInt32();
	}
}
