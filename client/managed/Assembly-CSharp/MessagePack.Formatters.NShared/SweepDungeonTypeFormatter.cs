using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SweepDungeonTypeFormatter : IMessagePackFormatter<SweepDungeonType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SweepDungeonType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public SweepDungeonType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (SweepDungeonType)reader.ReadInt32();
	}
}
