using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MultipleHeroSelectItemDataFormatter : IMessagePackFormatter<MultipleHeroSelectItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MultipleHeroSelectItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.HeroStar);
		writer.Write(value.HeroLevel);
		writer.Write(value.HeroTranscend);
		writer.Write(value.AllOwnedRewardIndex1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AllOwendRewardDesc1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SelectTitle1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SelectDesc1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HeroIndices1, options);
		writer.Write(value.AllOwnedRewardIndex2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AllOwendRewardDesc2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SelectTitle2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SelectDesc2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HeroIndices2, options);
	}

	public MultipleHeroSelectItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MultipleHeroSelectItemData multipleHeroSelectItemData = new MultipleHeroSelectItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				multipleHeroSelectItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				multipleHeroSelectItemData.HeroStar = reader.ReadInt32();
				break;
			case 2:
				multipleHeroSelectItemData.HeroLevel = reader.ReadInt32();
				break;
			case 3:
				multipleHeroSelectItemData.HeroTranscend = reader.ReadInt32();
				break;
			case 4:
				multipleHeroSelectItemData.AllOwnedRewardIndex1 = reader.ReadInt32();
				break;
			case 5:
				multipleHeroSelectItemData.AllOwendRewardDesc1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				multipleHeroSelectItemData.SelectTitle1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				multipleHeroSelectItemData.SelectDesc1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				multipleHeroSelectItemData.HeroIndices1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 9:
				multipleHeroSelectItemData.AllOwnedRewardIndex2 = reader.ReadInt32();
				break;
			case 10:
				multipleHeroSelectItemData.AllOwendRewardDesc2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				multipleHeroSelectItemData.SelectTitle2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				multipleHeroSelectItemData.SelectDesc2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				multipleHeroSelectItemData.HeroIndices2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return multipleHeroSelectItemData;
	}
}
