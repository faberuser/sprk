using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewPayShopTagDataFormatter : IMessagePackFormatter<NewPayShopTagData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewPayShopTagData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PayShopCategoryType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<PayShopIndexType>().Serialize(ref writer, value.IndexType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.Indices, options);
		writer.Write(value.Sort);
		writer.Write(value.IsEnable);
	}

	public NewPayShopTagData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewPayShopTagData newPayShopTagData = new NewPayShopTagData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newPayShopTagData.Index = reader.ReadInt32();
				break;
			case 1:
				newPayShopTagData.PayShopCategoryType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				newPayShopTagData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				newPayShopTagData.IndexType = resolver.GetFormatterWithVerify<PayShopIndexType>().Deserialize(ref reader, options);
				break;
			case 4:
				newPayShopTagData.Indices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				newPayShopTagData.Sort = reader.ReadInt32();
				break;
			case 6:
				newPayShopTagData.IsEnable = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newPayShopTagData;
	}
}
