using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LoadingData_EGroupTypeFormatter : IMessagePackFormatter<LoadingData.EGroupType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LoadingData.EGroupType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public LoadingData.EGroupType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (LoadingData.EGroupType)reader.ReadInt32();
	}
}
