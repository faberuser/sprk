using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StaminaValueTypeFormatter : IMessagePackFormatter<StaminaValueType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StaminaValueType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public StaminaValueType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (StaminaValueType)reader.ReadInt32();
	}
}
