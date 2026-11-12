using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LuckyArenaRewardDataFormatter : IMessagePackFormatter<LuckyArenaRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LuckyArenaRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<LuckyMatchRewardType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.ShowIndex);
		writer.Write(value.CountMin);
		writer.Write(value.CountMax);
		writer.Write(value.RewardIndex);
	}

	public LuckyArenaRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LuckyArenaRewardData luckyArenaRewardData = new LuckyArenaRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				luckyArenaRewardData.Type = resolver.GetFormatterWithVerify<LuckyMatchRewardType>().Deserialize(ref reader, options);
				break;
			case 1:
				luckyArenaRewardData.ShowIndex = reader.ReadInt32();
				break;
			case 2:
				luckyArenaRewardData.CountMin = reader.ReadInt32();
				break;
			case 3:
				luckyArenaRewardData.CountMax = reader.ReadInt32();
				break;
			case 4:
				luckyArenaRewardData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return luckyArenaRewardData;
	}
}
