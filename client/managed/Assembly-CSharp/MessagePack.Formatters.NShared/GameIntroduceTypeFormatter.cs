using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GameIntroduceTypeFormatter : IMessagePackFormatter<GameIntroduceType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GameIntroduceType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public GameIntroduceType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (GameIntroduceType)reader.ReadInt32();
	}
}
