using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AccessoryPartTypeFormatter : IMessagePackFormatter<AccessoryPartType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AccessoryPartType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public AccessoryPartType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (AccessoryPartType)reader.ReadInt32();
	}
}
