using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PayShopUserGroupFormatter : IMessagePackFormatter<PayShopUserGroup>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PayShopUserGroup value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PayShopUserGroup Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PayShopUserGroup)reader.ReadInt32();
	}
}
