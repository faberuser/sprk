using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CostumeSelectItemDataFormatter : IMessagePackFormatter<CostumeSelectItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CostumeSelectItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.CostumeCategories, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CostumeIndices, options);
		writer.Write(value.AllOwnedRewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AllOwnedRewardDesc, options);
		resolver.GetFormatterWithVerify<ECostumeSelectItemType>().Serialize(ref writer, value.CostumeSelectItemType, options);
	}

	public CostumeSelectItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CostumeSelectItemData costumeSelectItemData = new CostumeSelectItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				costumeSelectItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				costumeSelectItemData.CostumeCategories = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 2:
				costumeSelectItemData.CostumeIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				costumeSelectItemData.AllOwnedRewardIndex = reader.ReadInt32();
				break;
			case 4:
				costumeSelectItemData.AllOwnedRewardDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				costumeSelectItemData.CostumeSelectItemType = resolver.GetFormatterWithVerify<ECostumeSelectItemType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return costumeSelectItemData;
	}
}
