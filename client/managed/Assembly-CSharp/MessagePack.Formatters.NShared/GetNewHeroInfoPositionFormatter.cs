using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GetNewHeroInfoPositionFormatter : IMessagePackFormatter<GetNewHeroInfoPosition>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GetNewHeroInfoPosition value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public GetNewHeroInfoPosition Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (GetNewHeroInfoPosition)reader.ReadInt32();
	}
}
