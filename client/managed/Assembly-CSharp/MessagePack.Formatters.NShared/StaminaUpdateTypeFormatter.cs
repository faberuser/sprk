using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StaminaUpdateTypeFormatter : IMessagePackFormatter<StaminaUpdateType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StaminaUpdateType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public StaminaUpdateType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (StaminaUpdateType)reader.ReadInt32();
	}
}
