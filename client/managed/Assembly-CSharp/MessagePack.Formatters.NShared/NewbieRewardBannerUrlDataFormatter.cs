using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewbieRewardBannerUrlDataFormatter : IMessagePackFormatter<NewbieRewardBannerUrlData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewbieRewardBannerUrlData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<NewbieRewardType>().Serialize(ref writer, value.RewardType, options);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Language, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Url, options);
	}

	public NewbieRewardBannerUrlData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewbieRewardBannerUrlData newbieRewardBannerUrlData = new NewbieRewardBannerUrlData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newbieRewardBannerUrlData.RewardType = resolver.GetFormatterWithVerify<NewbieRewardType>().Deserialize(ref reader, options);
				break;
			case 1:
				newbieRewardBannerUrlData.ShowIndex = reader.ReadInt32();
				break;
			case 2:
				newbieRewardBannerUrlData.Language = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				newbieRewardBannerUrlData.Url = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newbieRewardBannerUrlData;
	}
}
