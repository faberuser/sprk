using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildBuildingDataFormatter : IMessagePackFormatter<GuildBuildingData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildBuildingData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(18);
		writer.Write(value.Index);
		writer.Write(value.Level);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ModelName, options);
		writer.Write(value.CanUpgrade);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IdleAnimationKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.UpgradeAnimationKey, options);
		writer.Write(value.ReqGuildLevel);
		writer.Write(value.ReqWood);
		writer.Write(value.ReqStone);
		writer.Write(value.ReqMetal);
		writer.Write(value.ShopIndex);
		writer.Write(value.GuildRaidRewardIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SkillIndex, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		writer.Write(value.ControlHeadQuarter);
	}

	public GuildBuildingData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildBuildingData guildBuildingData = new GuildBuildingData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildBuildingData.Index = reader.ReadInt32();
				break;
			case 1:
				guildBuildingData.Level = reader.ReadInt32();
				break;
			case 2:
				guildBuildingData.ModelName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				guildBuildingData.CanUpgrade = reader.ReadBoolean();
				break;
			case 4:
				guildBuildingData.IdleAnimationKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				guildBuildingData.UpgradeAnimationKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				guildBuildingData.ReqGuildLevel = reader.ReadInt32();
				break;
			case 7:
				guildBuildingData.ReqWood = reader.ReadInt32();
				break;
			case 8:
				guildBuildingData.ReqStone = reader.ReadInt32();
				break;
			case 9:
				guildBuildingData.ReqMetal = reader.ReadInt32();
				break;
			case 10:
				guildBuildingData.ShopIndex = reader.ReadInt32();
				break;
			case 11:
				guildBuildingData.GuildRaidRewardIndex = reader.ReadInt32();
				break;
			case 12:
				guildBuildingData.SkillIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 13:
				guildBuildingData.NameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				guildBuildingData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				guildBuildingData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				guildBuildingData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				guildBuildingData.ControlHeadQuarter = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildBuildingData;
	}
}
