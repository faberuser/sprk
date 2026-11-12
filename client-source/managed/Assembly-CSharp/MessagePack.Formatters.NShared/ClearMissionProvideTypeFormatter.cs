using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClearMissionProvideTypeFormatter : IMessagePackFormatter<ClearMissionProvideType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClearMissionProvideType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ClearMissionProvideType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ClearMissionProvideType)reader.ReadInt32();
	}
}
