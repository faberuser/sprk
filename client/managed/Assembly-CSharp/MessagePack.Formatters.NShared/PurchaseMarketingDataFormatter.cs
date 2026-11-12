using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PurchaseMarketingDataFormatter : IMessagePackFormatter<PurchaseMarketingData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PurchaseMarketingData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		writer.Write(value.RepeatCooltime);
		writer.Write(value.DurationTime);
		writer.Write(value.IsOpen);
		writer.Write(value.ProductIndex);
	}

	public PurchaseMarketingData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		PurchaseMarketingData purchaseMarketingData = new PurchaseMarketingData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				purchaseMarketingData.Index = reader.ReadInt32();
				break;
			case 1:
				purchaseMarketingData.RepeatCooltime = reader.ReadInt32();
				break;
			case 2:
				purchaseMarketingData.DurationTime = reader.ReadInt32();
				break;
			case 3:
				purchaseMarketingData.IsOpen = reader.ReadBoolean();
				break;
			case 4:
				purchaseMarketingData.ProductIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return purchaseMarketingData;
	}
}
