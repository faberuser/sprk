using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AccessoryPartPivotTypeFormatter : IMessagePackFormatter<AccessoryPartPivotType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AccessoryPartPivotType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public AccessoryPartPivotType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (AccessoryPartPivotType)reader.ReadInt32();
	}
}
