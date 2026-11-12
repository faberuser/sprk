using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LoadingData_ECategoryFormatter : IMessagePackFormatter<LoadingData.ECategory>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LoadingData.ECategory value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public LoadingData.ECategory Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (LoadingData.ECategory)reader.ReadInt32();
	}
}
