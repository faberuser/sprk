using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GachaSelectShopEffectDataFormatter : IMessagePackFormatter<GachaSelectShopEffectData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GachaSelectShopEffectData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemGroupCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.ItemCountMin);
	}

	public GachaSelectShopEffectData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GachaSelectShopEffectData gachaSelectShopEffectData = new GachaSelectShopEffectData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gachaSelectShopEffectData.ItemGroupCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				gachaSelectShopEffectData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				gachaSelectShopEffectData.ItemCountMin = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gachaSelectShopEffectData;
	}
}
