using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class InspectUserTypeFormatter : IMessagePackFormatter<InspectUserType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, InspectUserType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public InspectUserType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (InspectUserType)reader.ReadInt32();
	}
}
