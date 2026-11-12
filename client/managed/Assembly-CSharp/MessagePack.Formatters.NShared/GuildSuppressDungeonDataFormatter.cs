using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildSuppressDungeonDataFormatter : IMessagePackFormatter<GuildSuppressDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildSuppressDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		writer.Write(value.Index);
		writer.Write(value.DefaultRewardIndex);
		writer.Write(value.KillRewardIndex);
		writer.Write(value.KillActivityPoint);
		writer.Write(value.BuyableRewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Tactic, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RecommendedHeroIndex, options);
		writer.Write(value.BanIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewBg, options);
		writer.Write(value.FinishModeRatio);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ShowInfoSkill, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StatusBoardPortraitPath, options);
	}

	public GuildSuppressDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildSuppressDungeonData guildSuppressDungeonData = new GuildSuppressDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildSuppressDungeonData.Index = reader.ReadInt32();
				break;
			case 1:
				guildSuppressDungeonData.DefaultRewardIndex = reader.ReadInt32();
				break;
			case 2:
				guildSuppressDungeonData.KillRewardIndex = reader.ReadInt32();
				break;
			case 3:
				guildSuppressDungeonData.KillActivityPoint = reader.ReadInt32();
				break;
			case 4:
				guildSuppressDungeonData.BuyableRewardIndex = reader.ReadInt32();
				break;
			case 5:
				guildSuppressDungeonData.Tactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				guildSuppressDungeonData.RecommendedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 7:
				guildSuppressDungeonData.BanIndex = reader.ReadInt32();
				break;
			case 8:
				guildSuppressDungeonData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				guildSuppressDungeonData.PreviewBg = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				guildSuppressDungeonData.FinishModeRatio = reader.ReadInt64();
				break;
			case 11:
				guildSuppressDungeonData.ShowInfoSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				guildSuppressDungeonData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				guildSuppressDungeonData.StatusBoardPortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildSuppressDungeonData;
	}
}
