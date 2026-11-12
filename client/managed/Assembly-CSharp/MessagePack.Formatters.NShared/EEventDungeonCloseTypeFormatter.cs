using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EEventDungeonCloseTypeFormatter : IMessagePackFormatter<EEventDungeonCloseType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EEventDungeonCloseType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public EEventDungeonCloseType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (EEventDungeonCloseType)reader.ReadInt32();
	}
}
