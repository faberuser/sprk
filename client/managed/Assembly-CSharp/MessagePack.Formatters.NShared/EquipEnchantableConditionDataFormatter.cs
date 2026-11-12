using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipEnchantableConditionDataFormatter : IMessagePackFormatter<EquipEnchantableConditionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipEnchantableConditionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.TierIndex);
		writer.Write(value.DetailIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EnchantOptionItemCodes, options);
	}

	public EquipEnchantableConditionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipEnchantableConditionData equipEnchantableConditionData = new EquipEnchantableConditionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipEnchantableConditionData.TierIndex = reader.ReadInt32();
				break;
			case 1:
				equipEnchantableConditionData.DetailIndex = reader.ReadInt32();
				break;
			case 2:
				equipEnchantableConditionData.EnchantOptionItemCodes = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipEnchantableConditionData;
	}
}
