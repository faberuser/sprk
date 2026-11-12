using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GodkingTrialGroupDataFormatter : IMessagePackFormatter<GodkingTrialGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GodkingTrialGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(18);
		writer.Write(value.ChapterIndex);
		resolver.GetFormatterWithVerify<GodkingTrialDungeonGateType>().Serialize(ref writer, value.GateType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonNameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonNamePrefixKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossTitleNamePrefixKey, options);
		writer.Write(value.BanRuleIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BgNodeTreeCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BgNodeTreePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewShadowNodeTreePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewShadowNodeTreeCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewNodeTreePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewNodeTreeCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossDescription, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RecommendedHero, options);
		resolver.GetFormatterWithVerify<SkillAttrType[]>().Serialize(ref writer, value.GimicSkillAttrType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewBg, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Tactic, options);
		writer.Write(value.RewardIndex);
	}

	public GodkingTrialGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GodkingTrialGroupData godkingTrialGroupData = new GodkingTrialGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				godkingTrialGroupData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				godkingTrialGroupData.GateType = resolver.GetFormatterWithVerify<GodkingTrialDungeonGateType>().Deserialize(ref reader, options);
				break;
			case 2:
				godkingTrialGroupData.DungeonNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				godkingTrialGroupData.DungeonNamePrefixKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				godkingTrialGroupData.BossTitleNamePrefixKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				godkingTrialGroupData.BanRuleIndex = reader.ReadInt32();
				break;
			case 6:
				godkingTrialGroupData.BgNodeTreeCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				godkingTrialGroupData.BgNodeTreePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				godkingTrialGroupData.PreviewShadowNodeTreePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				godkingTrialGroupData.PreviewShadowNodeTreeCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				godkingTrialGroupData.PreviewNodeTreePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				godkingTrialGroupData.PreviewNodeTreeCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				godkingTrialGroupData.BossDescription = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				godkingTrialGroupData.RecommendedHero = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 14:
				godkingTrialGroupData.GimicSkillAttrType = resolver.GetFormatterWithVerify<SkillAttrType[]>().Deserialize(ref reader, options);
				break;
			case 15:
				godkingTrialGroupData.PreviewBg = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				godkingTrialGroupData.Tactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				godkingTrialGroupData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return godkingTrialGroupData;
	}
}
