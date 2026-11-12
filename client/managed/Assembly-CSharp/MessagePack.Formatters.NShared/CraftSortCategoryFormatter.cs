using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CraftSortCategoryFormatter : IMessagePackFormatter<CraftSortCategory>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CraftSortCategory value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public CraftSortCategory Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (CraftSortCategory)reader.ReadInt32();
	}
}
