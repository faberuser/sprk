using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ChapterClearRewardDataFormatter : IMessagePackFormatter<ChapterClearRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ChapterClearRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.ChapterIndex);
		writer.Write(value.StepIndex);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.RewardType, options);
		writer.Write(value.RewardCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardItemCode, options);
		writer.Write(value.RewardReqStar);
	}

	public ChapterClearRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ChapterClearRewardData chapterClearRewardData = new ChapterClearRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterClearRewardData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				chapterClearRewardData.StepIndex = reader.ReadInt32();
				break;
			case 2:
				chapterClearRewardData.RewardType = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 3:
				chapterClearRewardData.RewardCount = reader.ReadInt32();
				break;
			case 4:
				chapterClearRewardData.RewardItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				chapterClearRewardData.RewardReqStar = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return chapterClearRewardData;
	}
}
