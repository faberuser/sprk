using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewPayShopBannerDataFormatter : IMessagePackFormatter<NewPayShopBannerData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewPayShopBannerData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<PayShopUserGroup>().Serialize(ref writer, value.Group, options);
		resolver.GetFormatterWithVerify<BannerPlayTypes>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Url, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MoveCategory, options);
		writer.Write(value.OpenProductIndex);
		writer.Write(value.Sort);
		writer.Write(value.IsEnable);
	}

	public NewPayShopBannerData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewPayShopBannerData newPayShopBannerData = new NewPayShopBannerData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newPayShopBannerData.Index = reader.ReadInt32();
				break;
			case 1:
				newPayShopBannerData.Group = resolver.GetFormatterWithVerify<PayShopUserGroup>().Deserialize(ref reader, options);
				break;
			case 2:
				newPayShopBannerData.Type = resolver.GetFormatterWithVerify<BannerPlayTypes>().Deserialize(ref reader, options);
				break;
			case 3:
				newPayShopBannerData.Url = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				newPayShopBannerData.MoveCategory = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				newPayShopBannerData.OpenProductIndex = reader.ReadInt32();
				break;
			case 6:
				newPayShopBannerData.Sort = reader.ReadInt32();
				break;
			case 7:
				newPayShopBannerData.IsEnable = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newPayShopBannerData;
	}
}
