using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DungeonDifficultyTypeFormatter : IMessagePackFormatter<DungeonDifficultyType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DungeonDifficultyType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public DungeonDifficultyType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (DungeonDifficultyType)reader.ReadInt32();
	}
}
