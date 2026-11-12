using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AccumulateLoginRewardDataFormatter : IMessagePackFormatter<AccumulateLoginRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AccumulateLoginRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.Index);
		writer.Write(value.Day);
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

	public AccumulateLoginRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AccumulateLoginRewardData accumulateLoginRewardData = new AccumulateLoginRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				accumulateLoginRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				accumulateLoginRewardData.Day = reader.ReadInt32();
				break;
			case 2:
				accumulateLoginRewardData.Type1 = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 3:
				accumulateLoginRewardData.Value1 = reader.ReadInt32();
				break;
			case 4:
				accumulateLoginRewardData.Count1 = reader.ReadInt32();
				break;
			case 5:
				accumulateLoginRewardData.Type2 = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 6:
				accumulateLoginRewardData.Value2 = reader.ReadInt32();
				break;
			case 7:
				accumulateLoginRewardData.Count2 = reader.ReadInt32();
				break;
			case 8:
				accumulateLoginRewardData.Type3 = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 9:
				accumulateLoginRewardData.Value3 = reader.ReadInt32();
				break;
			case 10:
				accumulateLoginRewardData.Count3 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return accumulateLoginRewardData;
	}
}
