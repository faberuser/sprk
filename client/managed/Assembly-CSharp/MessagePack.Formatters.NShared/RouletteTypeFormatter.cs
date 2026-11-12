using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RouletteTypeFormatter : IMessagePackFormatter<RouletteType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RouletteType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RouletteType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RouletteType)reader.ReadInt32();
	}
}
