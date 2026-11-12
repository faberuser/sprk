using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroSelectItemDataFormatter : IMessagePackFormatter<HeroSelectItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroSelectItemData value, MessagePackSerializerOptions options)
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
		writer.Write(value.AllOwnedRewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AllOwnedRewardDesc, options);
		resolver.GetFormatterWithVerify<HeroSelectItemType>().Serialize(ref writer, value.HeroSelectItemType, options);
		writer.Write(value.NpcType);
	}

	public HeroSelectItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroSelectItemData heroSelectItemData = new HeroSelectItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroSelectItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				heroSelectItemData.HeroStar = reader.ReadInt32();
				break;
			case 2:
				heroSelectItemData.HeroLevel = reader.ReadInt32();
				break;
			case 3:
				heroSelectItemData.HeroTranscend = reader.ReadInt32();
				break;
			case 4:
				heroSelectItemData.HeroIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				heroSelectItemData.AllOwnedRewardIndex = reader.ReadInt32();
				break;
			case 6:
				heroSelectItemData.AllOwnedRewardDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				heroSelectItemData.HeroSelectItemType = resolver.GetFormatterWithVerify<HeroSelectItemType>().Deserialize(ref reader, options);
				break;
			case 8:
				heroSelectItemData.NpcType = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroSelectItemData;
	}
}
