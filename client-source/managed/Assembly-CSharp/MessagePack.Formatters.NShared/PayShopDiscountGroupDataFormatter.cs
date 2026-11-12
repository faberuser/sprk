using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PayShopDiscountGroupDataFormatter : IMessagePackFormatter<PayShopDiscountGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PayShopDiscountGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.GroupIndex);
		writer.Write(value.Step);
		writer.Write(value.TargetValue);
		writer.Write(value.Price);
	}

	public PayShopDiscountGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		PayShopDiscountGroupData payShopDiscountGroupData = new PayShopDiscountGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				payShopDiscountGroupData.GroupIndex = reader.ReadInt32();
				break;
			case 1:
				payShopDiscountGroupData.Step = reader.ReadInt32();
				break;
			case 2:
				payShopDiscountGroupData.TargetValue = reader.ReadInt32();
				break;
			case 3:
				payShopDiscountGroupData.Price = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return payShopDiscountGroupData;
	}
}
