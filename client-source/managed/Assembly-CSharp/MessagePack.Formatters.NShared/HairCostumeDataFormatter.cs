using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HairCostumeDataFormatter : IMessagePackFormatter<HairCostumeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HairCostumeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(19);
		writer.Write(value.Index);
		writer.Write(value.HeroIndex);
		writer.Write(value.Star);
		writer.WriteNil();
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Group, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HairPrefix, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.NeedCostume, options);
		writer.Write(value.ReqBuyGem);
		writer.Write(value.Mileage);
		writer.Write(value.IsDefault);
		writer.Write(value.IsOpen);
		writer.Write(value.PreviewableWhenNeedCostumeOwned);
		writer.Write(value.PreviewableWhenNeedHairOwned);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		writer.Write(value.IsNew);
		writer.Write(value.IsShow);
	}

	public HairCostumeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HairCostumeData hairCostumeData = new HairCostumeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				hairCostumeData.Index = reader.ReadInt32();
				break;
			case 1:
				hairCostumeData.HeroIndex = reader.ReadInt32();
				break;
			case 2:
				hairCostumeData.Star = reader.ReadInt32();
				break;
			case 5:
				hairCostumeData.Group = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				hairCostumeData.NameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				hairCostumeData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				hairCostumeData.HairPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				hairCostumeData.NeedCostume = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 10:
				hairCostumeData.ReqBuyGem = reader.ReadInt32();
				break;
			case 11:
				hairCostumeData.Mileage = reader.ReadInt32();
				break;
			case 12:
				hairCostumeData.IsDefault = reader.ReadBoolean();
				break;
			case 13:
				hairCostumeData.IsOpen = reader.ReadBoolean();
				break;
			case 14:
				hairCostumeData.PreviewableWhenNeedCostumeOwned = reader.ReadBoolean();
				break;
			case 15:
				hairCostumeData.PreviewableWhenNeedHairOwned = reader.ReadBoolean();
				break;
			case 16:
				hairCostumeData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				hairCostumeData.IsNew = reader.ReadBoolean();
				break;
			case 18:
				hairCostumeData.IsShow = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return hairCostumeData;
	}
}
