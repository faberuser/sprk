using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GetNewHeroSceneBundleDataFormatter : IMessagePackFormatter<GetNewHeroSceneBundleData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GetNewHeroSceneBundleData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.BundleHeroIndices, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PackCode, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TreeName, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SequenceHeroIndices, options);
		resolver.GetFormatterWithVerify<GetNewHeroInfoPosition[]>().Serialize(ref writer, value.HeroInfoPosition, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
	}

	public GetNewHeroSceneBundleData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GetNewHeroSceneBundleData getNewHeroSceneBundleData = new GetNewHeroSceneBundleData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				getNewHeroSceneBundleData.Index = reader.ReadInt32();
				break;
			case 1:
				getNewHeroSceneBundleData.BundleHeroIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 2:
				getNewHeroSceneBundleData.PackCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				getNewHeroSceneBundleData.TreeName = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				getNewHeroSceneBundleData.SequenceHeroIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				getNewHeroSceneBundleData.HeroInfoPosition = resolver.GetFormatterWithVerify<GetNewHeroInfoPosition[]>().Deserialize(ref reader, options);
				break;
			case 6:
				getNewHeroSceneBundleData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return getNewHeroSceneBundleData;
	}
}
