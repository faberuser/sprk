using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class FloorDungeonSelectTypeFormatter : IMessagePackFormatter<FloorDungeonSelectType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, FloorDungeonSelectType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public FloorDungeonSelectType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (FloorDungeonSelectType)reader.ReadInt32();
	}
}
