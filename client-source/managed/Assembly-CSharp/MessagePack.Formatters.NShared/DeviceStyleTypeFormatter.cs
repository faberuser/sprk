using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DeviceStyleTypeFormatter : IMessagePackFormatter<DeviceStyleType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DeviceStyleType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public DeviceStyleType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (DeviceStyleType)reader.ReadInt32();
	}
}
