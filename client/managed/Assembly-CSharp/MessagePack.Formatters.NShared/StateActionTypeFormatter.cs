using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StateActionTypeFormatter : IMessagePackFormatter<StateActionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StateActionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public StateActionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (StateActionType)reader.ReadInt32();
	}
}
