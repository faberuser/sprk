using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GameOptionTabFormatter : IMessagePackFormatter<GameOptionTab>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GameOptionTab value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public GameOptionTab Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (GameOptionTab)reader.ReadInt32();
	}
}
