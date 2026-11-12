using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BanRuleDataFormatter : IMessagePackFormatter<BanRuleData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BanRuleData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<RaidBanType>().Serialize(ref writer, value.BanType1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BanValue1, options);
		resolver.GetFormatterWithVerify<RaidBanType>().Serialize(ref writer, value.BanType2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BanValue2, options);
		resolver.GetFormatterWithVerify<RaidBanType>().Serialize(ref writer, value.BanType3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BanValue3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RuleName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RuleTactic, options);
		resolver.GetFormatterWithVerify<CreatureTagType[]>().Serialize(ref writer, value.SelectClassType, options);
		resolver.GetFormatterWithVerify<CreatureTagType[]>().Serialize(ref writer, value.AcceptedClassType, options);
	}

	public BanRuleData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		BanRuleData banRuleData = new BanRuleData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				banRuleData.Index = reader.ReadInt32();
				break;
			case 1:
				banRuleData.BanType1 = resolver.GetFormatterWithVerify<RaidBanType>().Deserialize(ref reader, options);
				break;
			case 2:
				banRuleData.BanValue1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				banRuleData.BanType2 = resolver.GetFormatterWithVerify<RaidBanType>().Deserialize(ref reader, options);
				break;
			case 4:
				banRuleData.BanValue2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				banRuleData.BanType3 = resolver.GetFormatterWithVerify<RaidBanType>().Deserialize(ref reader, options);
				break;
			case 6:
				banRuleData.BanValue3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				banRuleData.RuleName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				banRuleData.RuleTactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				banRuleData.SelectClassType = resolver.GetFormatterWithVerify<CreatureTagType[]>().Deserialize(ref reader, options);
				break;
			case 10:
				banRuleData.AcceptedClassType = resolver.GetFormatterWithVerify<CreatureTagType[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return banRuleData;
	}
}
