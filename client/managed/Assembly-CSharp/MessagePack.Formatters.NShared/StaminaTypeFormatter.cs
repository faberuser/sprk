using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StaminaTypeFormatter : IMessagePackFormatter<StaminaType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StaminaType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public StaminaType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (StaminaType)reader.ReadInt32();
	}
}
