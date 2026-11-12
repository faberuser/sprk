using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RuneSlotTypeFormatter : IMessagePackFormatter<RuneSlotType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RuneSlotType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public RuneSlotType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (RuneSlotType)reader.ReadInt32();
	}
}
