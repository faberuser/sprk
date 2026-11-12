using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClearMissionTypeFormatter : IMessagePackFormatter<ClearMissionType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClearMissionType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ClearMissionType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ClearMissionType)reader.ReadInt32();
	}
}
