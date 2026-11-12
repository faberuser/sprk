using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRaidRewardDataFormatter : IMessagePackFormatter<PunishmentRaidRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRaidRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		writer.Write(value.RaidIndex);
		writer.Write(value.RaidLevel);
		resolver.GetFormatterWithVerify<PunishmentRaidRewardType>().Serialize(ref writer, value.RewardType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ClearCheckIndex, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RewardIndex, options);
		writer.Write(value.Active);
		writer.Write(value.InfinityRewardGroupIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ShowRecieveSubRewardCondition, options);
	}

	public PunishmentRaidRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PunishmentRaidRewardData punishmentRaidRewardData = new PunishmentRaidRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentRaidRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				punishmentRaidRewardData.RaidIndex = reader.ReadInt32();
				break;
			case 2:
				punishmentRaidRewardData.RaidLevel = reader.ReadInt32();
				break;
			case 3:
				punishmentRaidRewardData.RewardType = resolver.GetFormatterWithVerify<PunishmentRaidRewardType>().Deserialize(ref reader, options);
				break;
			case 4:
				punishmentRaidRewardData.ClearCheckIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				punishmentRaidRewardData.RewardIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 6:
				punishmentRaidRewardData.Active = reader.ReadBoolean();
				break;
			case 7:
				punishmentRaidRewardData.InfinityRewardGroupIndex = reader.ReadInt32();
				break;
			case 8:
				punishmentRaidRewardData.ShowRecieveSubRewardCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return punishmentRaidRewardData;
	}
}
