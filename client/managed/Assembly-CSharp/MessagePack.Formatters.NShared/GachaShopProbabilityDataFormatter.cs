using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GachaShopProbabilityDataFormatter : IMessagePackFormatter<GachaShopProbabilityData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GachaShopProbabilityData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.GachaIndex);
		resolver.GetFormatterWithVerify<HighGachaCategoryType>().Serialize(ref writer, value.GachaType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ProbabilityIndices, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.GachaNames, options);
	}

	public GachaShopProbabilityData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GachaShopProbabilityData gachaShopProbabilityData = new GachaShopProbabilityData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gachaShopProbabilityData.GachaIndex = reader.ReadInt32();
				break;
			case 1:
				gachaShopProbabilityData.GachaType = resolver.GetFormatterWithVerify<HighGachaCategoryType>().Deserialize(ref reader, options);
				break;
			case 2:
				gachaShopProbabilityData.ProbabilityIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				gachaShopProbabilityData.GachaNames = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gachaShopProbabilityData;
	}
}
