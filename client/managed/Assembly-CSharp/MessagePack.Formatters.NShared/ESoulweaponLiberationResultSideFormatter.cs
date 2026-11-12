using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ESoulweaponLiberationResultSideFormatter : IMessagePackFormatter<ESoulweaponLiberationResultSide>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ESoulweaponLiberationResultSide value, MessagePackSerializerOptions options)
	{
		writer.Write((int)value);
	}

	public ESoulweaponLiberationResultSide Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		return (ESoulweaponLiberationResultSide)reader.ReadInt32();
	}
}
