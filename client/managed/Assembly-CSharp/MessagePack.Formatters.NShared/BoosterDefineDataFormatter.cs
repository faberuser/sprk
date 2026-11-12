using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BoosterDefineDataFormatter : IMessagePackFormatter<BoosterDefineData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BoosterDefineData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BoosterCode, options);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.BonusType, options);
		resolver.GetFormatterWithVerify<ECostumeBuffType>().Serialize(ref writer, value.CostumeBuffType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TooltipName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TooltipDesc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PopUpBuffCategory, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PopUpBuffDetail, options);
	}

	public BoosterDefineData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		BoosterDefineData boosterDefineData = new BoosterDefineData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				boosterDefineData.BoosterCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				boosterDefineData.BonusType = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 2:
				boosterDefineData.CostumeBuffType = resolver.GetFormatterWithVerify<ECostumeBuffType>().Deserialize(ref reader, options);
				break;
			case 3:
				boosterDefineData.IconItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				boosterDefineData.TooltipName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				boosterDefineData.TooltipDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				boosterDefineData.PopUpBuffCategory = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				boosterDefineData.PopUpBuffDetail = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return boosterDefineData;
	}
}
