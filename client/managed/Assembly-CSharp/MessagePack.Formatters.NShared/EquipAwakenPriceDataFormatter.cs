using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipAwakenPriceDataFormatter : IMessagePackFormatter<EquipAwakenPriceData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipAwakenPriceData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.DetailIndex);
		writer.Write(value.Star);
		writer.Write(value.Tier);
		writer.Write(value.ReqGold);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode1, options);
		writer.Write(value.ItemCount1);
	}

	public EquipAwakenPriceData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipAwakenPriceData equipAwakenPriceData = new EquipAwakenPriceData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipAwakenPriceData.DetailIndex = reader.ReadInt32();
				break;
			case 1:
				equipAwakenPriceData.Star = reader.ReadInt32();
				break;
			case 2:
				equipAwakenPriceData.Tier = reader.ReadInt32();
				break;
			case 3:
				equipAwakenPriceData.ReqGold = reader.ReadInt32();
				break;
			case 4:
				equipAwakenPriceData.ItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				equipAwakenPriceData.ItemCount1 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipAwakenPriceData;
	}
}
