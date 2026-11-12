using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClientActivityTypeFormatter : IMessagePackFormatter<ClientActivityType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClientActivityType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ClientActivityType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ClientActivityType)reader.ReadInt32();
	}
}
