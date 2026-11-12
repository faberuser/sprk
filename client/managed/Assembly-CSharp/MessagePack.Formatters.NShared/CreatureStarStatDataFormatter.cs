using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureStarStatDataFormatter : IMessagePackFormatter<CreatureStarStatData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureStarStatData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Star);
		writer.Write(value.Transcended);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.StatType, options);
		writer.Write(value.StatFactor);
	}

	public CreatureStarStatData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureStarStatData creatureStarStatData = new CreatureStarStatData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureStarStatData.Star = reader.ReadInt32();
				break;
			case 1:
				creatureStarStatData.Transcended = reader.ReadInt32();
				break;
			case 2:
				creatureStarStatData.StatType = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 3:
				creatureStarStatData.StatFactor = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureStarStatData;
	}
}
