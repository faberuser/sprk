using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PortalRenewalCompTypeFormatter : IMessagePackFormatter<PortalRenewalCompType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PortalRenewalCompType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PortalRenewalCompType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PortalRenewalCompType)reader.ReadInt32();
	}
}
