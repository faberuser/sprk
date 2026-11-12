using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PotionItemDataFormatter : IMessagePackFormatter<PotionItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PotionItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<ItemActionType>().Serialize(ref writer, value.ActionType, options);
		writer.Write(value.ActionValue);
		writer.Write(value.ActionValue2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ActionSubValue, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BoosterCodes, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.UIEffectPrefabName, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.UIEffectValue, options);
		writer.Write(value.AutoUse);
		resolver.GetFormatterWithVerify<ResetType>().Serialize(ref writer, value.StaminaResetWarnningType, options);
		writer.Write(value.StaminaResetWarnningValue);
	}

	public PotionItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PotionItemData potionItemData = new PotionItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				potionItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				potionItemData.ActionType = resolver.GetFormatterWithVerify<ItemActionType>().Deserialize(ref reader, options);
				break;
			case 2:
				potionItemData.ActionValue = reader.ReadInt32();
				break;
			case 3:
				potionItemData.ActionValue2 = reader.ReadInt32();
				break;
			case 4:
				potionItemData.ActionSubValue = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				potionItemData.BoosterCodes = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 6:
				potionItemData.UIEffectPrefabName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				potionItemData.UIEffectValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 8:
				potionItemData.AutoUse = reader.ReadBoolean();
				break;
			case 9:
				potionItemData.StaminaResetWarnningType = resolver.GetFormatterWithVerify<ResetType>().Deserialize(ref reader, options);
				break;
			case 10:
				potionItemData.StaminaResetWarnningValue = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return potionItemData;
	}
}
