using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GachaSelectShopProbabilityDataFormatter : IMessagePackFormatter<GachaSelectShopProbabilityData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GachaSelectShopProbabilityData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.SelectShopIndex);
		writer.Write(value.SubIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentTitle, options);
		writer.Write(value.ComponentCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentRate, options);
	}

	public GachaSelectShopProbabilityData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GachaSelectShopProbabilityData gachaSelectShopProbabilityData = new GachaSelectShopProbabilityData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gachaSelectShopProbabilityData.SelectShopIndex = reader.ReadInt32();
				break;
			case 1:
				gachaSelectShopProbabilityData.SubIndex = reader.ReadInt32();
				break;
			case 2:
				gachaSelectShopProbabilityData.ComponentTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				gachaSelectShopProbabilityData.ComponentCount = reader.ReadInt32();
				break;
			case 4:
				gachaSelectShopProbabilityData.ComponentRate = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gachaSelectShopProbabilityData;
	}
}
