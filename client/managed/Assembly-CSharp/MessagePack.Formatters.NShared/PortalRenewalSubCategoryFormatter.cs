using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PortalRenewalSubCategoryFormatter : IMessagePackFormatter<PortalRenewalSubCategory>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PortalRenewalSubCategory value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PortalRenewalSubCategory Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PortalRenewalSubCategory)reader.ReadInt32();
	}
}
