using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaRewardKeyFormatter : IMessagePackFormatter<KarmaRewardKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaRewardKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.RewardIndex);
	}

	public KarmaRewardKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int punishmentGroupIndex = 0;
		int rewardIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				rewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		KarmaRewardKey result = new KarmaRewardKey(punishmentGroupIndex, rewardIndex);
		reader.Depth--;
		return result;
	}
}
