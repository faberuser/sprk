using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RouletteRewardDataFormatter : IMessagePackFormatter<RouletteRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RouletteRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		writer.Write(value.Index);
		writer.Write(value.Section);
		writer.Write(value.Rate);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.Type1, options);
		writer.Write(value.Value1);
		writer.Write(value.Count1);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.Type2, options);
		writer.Write(value.Value2);
		writer.Write(value.Count2);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.Type3, options);
		writer.Write(value.Value3);
		writer.Write(value.Count3);
	}

	public RouletteRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RouletteRewardData rouletteRewardData = new RouletteRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				rouletteRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				rouletteRewardData.Section = reader.ReadInt32();
				break;
			case 2:
				rouletteRewardData.Rate = reader.ReadInt32();
				break;
			case 3:
				rouletteRewardData.Type1 = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 4:
				rouletteRewardData.Value1 = reader.ReadInt32();
				break;
			case 5:
				rouletteRewardData.Count1 = reader.ReadInt32();
				break;
			case 6:
				rouletteRewardData.Type2 = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 7:
				rouletteRewardData.Value2 = reader.ReadInt32();
				break;
			case 8:
				rouletteRewardData.Count2 = reader.ReadInt32();
				break;
			case 9:
				rouletteRewardData.Type3 = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 10:
				rouletteRewardData.Value3 = reader.ReadInt32();
				break;
			case 11:
				rouletteRewardData.Count3 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return rouletteRewardData;
	}
}
