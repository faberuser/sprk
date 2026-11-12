using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PortalPropConditionTypeFormatter : IMessagePackFormatter<PortalPropConditionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PortalPropConditionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PortalPropConditionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PortalPropConditionType)reader.ReadInt32();
	}
}
