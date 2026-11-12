using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class InspectUserCategoryFormatter : IMessagePackFormatter<InspectUserCategory>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, InspectUserCategory value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public InspectUserCategory Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (InspectUserCategory)reader.ReadInt32();
	}
}
