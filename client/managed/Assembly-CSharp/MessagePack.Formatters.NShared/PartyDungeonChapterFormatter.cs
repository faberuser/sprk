using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PartyDungeonChapterFormatter : IMessagePackFormatter<PartyDungeonChapter>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PartyDungeonChapter value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PartyDungeonChapter Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PartyDungeonChapter)reader.ReadInt32();
	}
}
