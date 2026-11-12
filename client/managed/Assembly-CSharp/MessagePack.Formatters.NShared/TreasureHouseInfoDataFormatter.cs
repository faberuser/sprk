using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TreasureHouseInfoDataFormatter : IMessagePackFormatter<TreasureHouseInfoData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TreasureHouseInfoData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		writer.Write(value.EnterCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ContentsDesc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ChangeCurrencyType, options);
		writer.Write(value.ChangeCurrencyCost);
	}

	public TreasureHouseInfoData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		TreasureHouseInfoData treasureHouseInfoData = new TreasureHouseInfoData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				treasureHouseInfoData.Index = reader.ReadInt32();
				break;
			case 1:
				treasureHouseInfoData.EnterCount = reader.ReadInt32();
				break;
			case 2:
				treasureHouseInfoData.ContentsDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				treasureHouseInfoData.ChangeCurrencyType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				treasureHouseInfoData.ChangeCurrencyCost = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return treasureHouseInfoData;
	}
}
