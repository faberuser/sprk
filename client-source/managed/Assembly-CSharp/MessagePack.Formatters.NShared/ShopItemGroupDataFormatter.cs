using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShopItemGroupDataFormatter : IMessagePackFormatter<ShopItemGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShopItemGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.ShopIndex);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GroupNameKey, options);
		writer.Write(value.IsDefault);
	}

	public ShopItemGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShopItemGroupData shopItemGroupData = new ShopItemGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shopItemGroupData.ShopIndex = reader.ReadInt32();
				break;
			case 1:
				shopItemGroupData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				shopItemGroupData.GroupNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				shopItemGroupData.IsDefault = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shopItemGroupData;
	}
}
