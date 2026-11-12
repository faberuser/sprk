using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentGroupDataFormatter : IMessagePackFormatter<PunishmentGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(25);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<EPunishmentDungeonType>().Serialize(ref writer, value.PunishmentDungeonType, options);
		writer.Write(value.ChapterIndex);
		writer.Write(value.KarmaRaidIndex);
		writer.Write(value.OpenStaminaCount);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.StaminaIncreaseCount, options);
		writer.Write(value.StaminaIncreaseRate);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ImagePath, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BackgroundPaths, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NoticeInfoKey, options);
		writer.Write(value.AffixGroupIndex);
		writer.Write(value.InfinityRewardGroupIndex);
		writer.Write(value.CreatureIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CreaturePosition, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CreatureRotation, options);
		writer.Write(value.CreatureScale);
		writer.Write(value.KarmaFixRewardIndex);
		writer.Write(value.KarmaBonusRewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossGuidanceKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardDescKey, options);
		writer.Write(value.InitialShardPoint);
		writer.Write(value.KarmaCardPickInterval);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossRaidBgm, options);
	}

	public PunishmentGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PunishmentGroupData punishmentGroupData = new PunishmentGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentGroupData.GroupIndex = reader.ReadInt32();
				break;
			case 1:
				punishmentGroupData.PunishmentDungeonType = resolver.GetFormatterWithVerify<EPunishmentDungeonType>().Deserialize(ref reader, options);
				break;
			case 2:
				punishmentGroupData.ChapterIndex = reader.ReadInt32();
				break;
			case 3:
				punishmentGroupData.KarmaRaidIndex = reader.ReadInt32();
				break;
			case 4:
				punishmentGroupData.OpenStaminaCount = reader.ReadInt32();
				break;
			case 5:
				punishmentGroupData.StaminaIncreaseCount = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 6:
				punishmentGroupData.StaminaIncreaseRate = reader.ReadInt32();
				break;
			case 7:
				punishmentGroupData.NameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				punishmentGroupData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				punishmentGroupData.ImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				punishmentGroupData.BackgroundPaths = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 11:
				punishmentGroupData.NoticeInfoKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				punishmentGroupData.AffixGroupIndex = reader.ReadInt32();
				break;
			case 13:
				punishmentGroupData.InfinityRewardGroupIndex = reader.ReadInt32();
				break;
			case 14:
				punishmentGroupData.CreatureIndex = reader.ReadInt32();
				break;
			case 15:
				punishmentGroupData.CreaturePosition = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 16:
				punishmentGroupData.CreatureRotation = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 17:
				punishmentGroupData.CreatureScale = reader.ReadInt32();
				break;
			case 18:
				punishmentGroupData.KarmaFixRewardIndex = reader.ReadInt32();
				break;
			case 19:
				punishmentGroupData.KarmaBonusRewardIndex = reader.ReadInt32();
				break;
			case 20:
				punishmentGroupData.BossGuidanceKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 21:
				punishmentGroupData.RewardDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				punishmentGroupData.InitialShardPoint = reader.ReadInt32();
				break;
			case 23:
				punishmentGroupData.KarmaCardPickInterval = reader.ReadInt32();
				break;
			case 24:
				punishmentGroupData.BossRaidBgm = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return punishmentGroupData;
	}
}
