using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KingsPassSeasonMissionDataContainer_RewardKey2Formatter : IMessagePackFormatter<KingsPassSeasonMissionDataContainer.RewardKey2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KingsPassSeasonMissionDataContainer.RewardKey2 value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.SeasonIndex);
		resolver.GetFormatterWithVerify<KingsPassType>().Serialize(ref writer, value.PassType, options);
		writer.Write(value.Level);
	}

	public KingsPassSeasonMissionDataContainer.RewardKey2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int seasonIndex = 0;
		KingsPassType passType = KingsPassType.None;
		int level = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				seasonIndex = reader.ReadInt32();
				break;
			case 1:
				passType = resolver.GetFormatterWithVerify<KingsPassType>().Deserialize(ref reader, options);
				break;
			case 2:
				level = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		KingsPassSeasonMissionDataContainer.RewardKey2 result = new KingsPassSeasonMissionDataContainer.RewardKey2(seasonIndex, passType, level);
		reader.Depth--;
		return result;
	}
}
