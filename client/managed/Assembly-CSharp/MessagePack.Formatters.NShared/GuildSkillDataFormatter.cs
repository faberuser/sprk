using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildSkillDataFormatter : IMessagePackFormatter<GuildSkillData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildSkillData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.GuildSkillIndex);
		resolver.GetFormatterWithVerify<GuildSkillType>().Serialize(ref writer, value.GuildSkillType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillDescParam, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillDesc, options);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.BonusType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ToolTipName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ToolTipDesc, options);
		resolver.GetFormatterWithVerify<BattleType[]>().Serialize(ref writer, value.BattleTypes, options);
	}

	public GuildSkillData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildSkillData guildSkillData = new GuildSkillData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildSkillData.GuildSkillIndex = reader.ReadInt32();
				break;
			case 1:
				guildSkillData.GuildSkillType = resolver.GetFormatterWithVerify<GuildSkillType>().Deserialize(ref reader, options);
				break;
			case 2:
				guildSkillData.SkillDescParam = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				guildSkillData.SkillDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				guildSkillData.BonusType = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 5:
				guildSkillData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				guildSkillData.ToolTipName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				guildSkillData.ToolTipDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				guildSkillData.BattleTypes = resolver.GetFormatterWithVerify<BattleType[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildSkillData;
	}
}
