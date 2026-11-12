using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StateFlagFormatter : IMessagePackFormatter<StateFlag>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StateFlag value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public StateFlag Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (StateFlag)reader.ReadInt32();
	}
}
