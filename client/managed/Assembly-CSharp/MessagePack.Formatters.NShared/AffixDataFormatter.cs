using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AffixDataFormatter : IMessagePackFormatter<AffixData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AffixData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(17);
		writer.Write(value.Index);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<EAffixType>().Serialize(ref writer, value.AffixType, options);
		resolver.GetFormatterWithVerify<TeamType>().Serialize(ref writer, value.TeamType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HeroIndices, options);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.OptionType, options);
		writer.Write(value.OptionValue);
		writer.Write(value.Point);
		writer.Write(value.IsPenalty);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
		writer.Write(value.BanRuleIndex);
		resolver.GetFormatterWithVerify<EAffixShowType>().Serialize(ref writer, value.ShowType, options);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.AutoApplyConditionValues, options);
		writer.Write(value.InfinityIndex);
		writer.Write(value.InfinityStartLevel);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
	}

	public AffixData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AffixData affixData = new AffixData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				affixData.Index = reader.ReadInt32();
				break;
			case 1:
				affixData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				affixData.AffixType = resolver.GetFormatterWithVerify<EAffixType>().Deserialize(ref reader, options);
				break;
			case 3:
				affixData.TeamType = resolver.GetFormatterWithVerify<TeamType>().Deserialize(ref reader, options);
				break;
			case 4:
				affixData.HeroIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				affixData.OptionType = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 6:
				affixData.OptionValue = reader.ReadInt32();
				break;
			case 7:
				affixData.Point = reader.ReadInt32();
				break;
			case 8:
				affixData.IsPenalty = reader.ReadBoolean();
				break;
			case 9:
				affixData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				affixData.BanRuleIndex = reader.ReadInt32();
				break;
			case 11:
				affixData.ShowType = resolver.GetFormatterWithVerify<EAffixShowType>().Deserialize(ref reader, options);
				break;
			case 13:
				affixData.AutoApplyConditionValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				affixData.InfinityIndex = reader.ReadInt32();
				break;
			case 15:
				affixData.InfinityStartLevel = reader.ReadInt32();
				break;
			case 16:
				affixData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return affixData;
	}
}
