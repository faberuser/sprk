using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GameOptionComponentFormatter : IMessagePackFormatter<GameOptionComponent>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GameOptionComponent value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public GameOptionComponent Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (GameOptionComponent)reader.ReadInt32();
	}
}
