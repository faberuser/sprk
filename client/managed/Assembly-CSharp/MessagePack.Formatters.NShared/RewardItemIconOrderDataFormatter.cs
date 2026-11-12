using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RewardItemIconOrderDataFormatter : IMessagePackFormatter<RewardItemIconOrderData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RewardItemIconOrderData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<RewardItemIconType>().Serialize(ref writer, value.RewardItemIconType, options);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.CurrencyType, options);
		writer.Write(value.Priority);
		writer.Write(value.Sum);
	}

	public RewardItemIconOrderData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RewardItemIconOrderData rewardItemIconOrderData = new RewardItemIconOrderData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				rewardItemIconOrderData.RewardItemIconType = resolver.GetFormatterWithVerify<RewardItemIconType>().Deserialize(ref reader, options);
				break;
			case 1:
				rewardItemIconOrderData.CurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 2:
				rewardItemIconOrderData.Priority = reader.ReadInt32();
				break;
			case 3:
				rewardItemIconOrderData.Sum = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return rewardItemIconOrderData;
	}
}
