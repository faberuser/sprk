using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PlayGuideData_GuideTypeFormatter : IMessagePackFormatter<PlayGuideData.GuideType>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PlayGuideData.GuideType value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public PlayGuideData.GuideType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (PlayGuideData.GuideType)reader.ReadInt32();
	}
}
