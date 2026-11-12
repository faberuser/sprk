using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipUpgradeTierDataFormatter : IMessagePackFormatter<EquipUpgradeTierData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipUpgradeTierData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ResultItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode1, options);
		writer.Write(value.MaterialItemCount1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode2, options);
		writer.Write(value.MaterialItemCount2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode3, options);
		writer.Write(value.MaterialItemCount3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode4, options);
		writer.Write(value.MaterialItemCount4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode5, options);
		writer.Write(value.MaterialItemCount5);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.CostType, options);
		writer.Write(value.CostValue);
	}

	public EquipUpgradeTierData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipUpgradeTierData equipUpgradeTierData = new EquipUpgradeTierData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipUpgradeTierData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				equipUpgradeTierData.ResultItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				equipUpgradeTierData.MaterialItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				equipUpgradeTierData.MaterialItemCount1 = reader.ReadInt32();
				break;
			case 4:
				equipUpgradeTierData.MaterialItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				equipUpgradeTierData.MaterialItemCount2 = reader.ReadInt32();
				break;
			case 6:
				equipUpgradeTierData.MaterialItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				equipUpgradeTierData.MaterialItemCount3 = reader.ReadInt32();
				break;
			case 8:
				equipUpgradeTierData.MaterialItemCode4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				equipUpgradeTierData.MaterialItemCount4 = reader.ReadInt32();
				break;
			case 10:
				equipUpgradeTierData.MaterialItemCode5 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				equipUpgradeTierData.MaterialItemCount5 = reader.ReadInt32();
				break;
			case 12:
				equipUpgradeTierData.CostType = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 13:
				equipUpgradeTierData.CostValue = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipUpgradeTierData;
	}
}
