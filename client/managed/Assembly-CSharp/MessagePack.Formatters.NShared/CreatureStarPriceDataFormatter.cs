using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureStarPriceDataFormatter : IMessagePackFormatter<CreatureStarPriceData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureStarPriceData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<CreatureOpenStatusType>().Serialize(ref writer, value.OpenStatus, options);
		writer.Write(value.Star);
		writer.Write(value.BuyGem);
		writer.Write(value.Mileage);
	}

	public CreatureStarPriceData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureStarPriceData creatureStarPriceData = new CreatureStarPriceData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureStarPriceData.OpenStatus = resolver.GetFormatterWithVerify<CreatureOpenStatusType>().Deserialize(ref reader, options);
				break;
			case 1:
				creatureStarPriceData.Star = reader.ReadInt32();
				break;
			case 2:
				creatureStarPriceData.BuyGem = reader.ReadInt32();
				break;
			case 3:
				creatureStarPriceData.Mileage = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureStarPriceData;
	}
}
