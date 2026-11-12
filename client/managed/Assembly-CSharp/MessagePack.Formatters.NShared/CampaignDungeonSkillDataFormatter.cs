using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignDungeonSkillDataFormatter : IMessagePackFormatter<CampaignDungeonSkillData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignDungeonSkillData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.BaseSkillIndex);
		writer.Write(value.EnhancedSkillIndex);
		writer.Write(value.MaxUseCount);
		resolver.GetFormatterWithVerify<TeamType>().Serialize(ref writer, value.TeamType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BaseCheckState, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EnhancedCheckState, options);
	}

	public CampaignDungeonSkillData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignDungeonSkillData campaignDungeonSkillData = new CampaignDungeonSkillData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				campaignDungeonSkillData.BaseSkillIndex = reader.ReadInt32();
				break;
			case 1:
				campaignDungeonSkillData.EnhancedSkillIndex = reader.ReadInt32();
				break;
			case 2:
				campaignDungeonSkillData.MaxUseCount = reader.ReadInt32();
				break;
			case 3:
				campaignDungeonSkillData.TeamType = resolver.GetFormatterWithVerify<TeamType>().Deserialize(ref reader, options);
				break;
			case 4:
				campaignDungeonSkillData.BaseCheckState = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				campaignDungeonSkillData.EnhancedCheckState = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return campaignDungeonSkillData;
	}
}
