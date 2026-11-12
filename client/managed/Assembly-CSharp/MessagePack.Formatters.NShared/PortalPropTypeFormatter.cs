using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PortalPropTypeFormatter : IMessagePackFormatter<PortalPropType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PortalPropType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PortalPropType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PortalPropType)reader.ReadInt32();
	}
}
