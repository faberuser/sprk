using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DispatchStateFormatter : IMessagePackFormatter<DispatchState>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DispatchState value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public DispatchState Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (DispatchState)reader.ReadInt32();
	}
}
