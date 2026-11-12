using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipEnchantOptionItemDataFormatter : IMessagePackFormatter<EquipEnchantOptionItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipEnchantOptionItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.ReqCount);
		writer.Write(value.ReqGold);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.EnchantOptionIndex1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.EnchantOptionIndex2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.EnchantOptionIndex3, options);
	}

	public EquipEnchantOptionItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipEnchantOptionItemData equipEnchantOptionItemData = new EquipEnchantOptionItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipEnchantOptionItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				equipEnchantOptionItemData.ReqCount = reader.ReadInt32();
				break;
			case 2:
				equipEnchantOptionItemData.ReqGold = reader.ReadInt32();
				break;
			case 3:
				equipEnchantOptionItemData.EnchantOptionIndex1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 4:
				equipEnchantOptionItemData.EnchantOptionIndex2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				equipEnchantOptionItemData.EnchantOptionIndex3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipEnchantOptionItemData;
	}
}
