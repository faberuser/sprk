using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewbieRewardBannerUrlKeyFormatter : IMessagePackFormatter<NewbieRewardBannerUrlKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewbieRewardBannerUrlKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<NewbieRewardType>().Serialize(ref writer, value.RewardType, options);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Language, options);
	}

	public NewbieRewardBannerUrlKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewbieRewardType rewardType = NewbieRewardType.None;
		int showIndex = 0;
		string language = null;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				rewardType = resolver.GetFormatterWithVerify<NewbieRewardType>().Deserialize(ref reader, options);
				break;
			case 1:
				showIndex = reader.ReadInt32();
				break;
			case 2:
				language = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		NewbieRewardBannerUrlKey result = new NewbieRewardBannerUrlKey(rewardType, showIndex, language);
		reader.Depth--;
		return result;
	}
}
