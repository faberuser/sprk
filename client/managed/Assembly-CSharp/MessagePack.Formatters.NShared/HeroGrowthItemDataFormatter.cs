using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroGrowthItemDataFormatter : IMessagePackFormatter<HeroGrowthItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroGrowthItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.HeroStar);
		writer.Write(value.HeroLevel);
		writer.Write(value.HeroTranscend);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HeroIndices, options);
		writer.Write(value.OverWritteHeroStar);
		writer.Write(value.OverWritteHeroLevel);
		writer.Write(value.OverWritteHeroTranscend);
		writer.Write(value.AllGrowthRewardIndex);
	}

	public HeroGrowthItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroGrowthItemData heroGrowthItemData = new HeroGrowthItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroGrowthItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				heroGrowthItemData.HeroStar = reader.ReadInt32();
				break;
			case 2:
				heroGrowthItemData.HeroLevel = reader.ReadInt32();
				break;
			case 3:
				heroGrowthItemData.HeroTranscend = reader.ReadInt32();
				break;
			case 4:
				heroGrowthItemData.HeroIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				heroGrowthItemData.OverWritteHeroStar = reader.ReadBoolean();
				break;
			case 6:
				heroGrowthItemData.OverWritteHeroLevel = reader.ReadBoolean();
				break;
			case 7:
				heroGrowthItemData.OverWritteHeroTranscend = reader.ReadBoolean();
				break;
			case 8:
				heroGrowthItemData.AllGrowthRewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroGrowthItemData;
	}
}
