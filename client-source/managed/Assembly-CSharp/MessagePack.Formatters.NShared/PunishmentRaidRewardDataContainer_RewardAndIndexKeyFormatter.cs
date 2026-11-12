using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRaidRewardDataContainer_RewardAndIndexKeyFormatter : IMessagePackFormatter<PunishmentRaidRewardDataContainer.RewardAndIndexKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRaidRewardDataContainer.RewardAndIndexKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<PunishmentRaidRewardType>().Serialize(ref writer, value.RewardType, options);
		writer.Write(value.RaidIndex);
	}

	public PunishmentRaidRewardDataContainer.RewardAndIndexKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PunishmentRaidRewardType rewardType = PunishmentRaidRewardType.None;
		int raidIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				rewardType = resolver.GetFormatterWithVerify<PunishmentRaidRewardType>().Deserialize(ref reader, options);
				break;
			case 1:
				raidIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		PunishmentRaidRewardDataContainer.RewardAndIndexKey result = new PunishmentRaidRewardDataContainer.RewardAndIndexKey(rewardType, raidIndex);
		reader.Depth--;
		return result;
	}
}
