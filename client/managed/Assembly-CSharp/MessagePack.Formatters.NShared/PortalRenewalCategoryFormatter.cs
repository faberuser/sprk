using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PortalRenewalCategoryFormatter : IMessagePackFormatter<PortalRenewalCategory>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PortalRenewalCategory value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PortalRenewalCategory Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PortalRenewalCategory)reader.ReadInt32();
	}
}
