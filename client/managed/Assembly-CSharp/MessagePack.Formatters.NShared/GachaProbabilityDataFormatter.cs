using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GachaProbabilityDataFormatter : IMessagePackFormatter<GachaProbabilityData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GachaProbabilityData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.GachaIndex);
		writer.Write(value.SubIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentTitle, options);
		writer.Write(value.ComponentCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentRate, options);
		writer.Write(value.InformIndex);
	}

	public GachaProbabilityData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GachaProbabilityData gachaProbabilityData = new GachaProbabilityData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gachaProbabilityData.GachaIndex = reader.ReadInt32();
				break;
			case 1:
				gachaProbabilityData.SubIndex = reader.ReadInt32();
				break;
			case 2:
				gachaProbabilityData.ComponentTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				gachaProbabilityData.ComponentCount = reader.ReadInt32();
				break;
			case 4:
				gachaProbabilityData.ComponentRate = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				gachaProbabilityData.InformIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gachaProbabilityData;
	}
}
