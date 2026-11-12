using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LoadingDataFormatter : IMessagePackFormatter<LoadingData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LoadingData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<LoadingData.ECategory>().Serialize(ref writer, value.Category, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Argument, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroCode, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.HeroPosOffset, options);
		writer.Write(value.HeroWidth);
		writer.Write(value.HeroHeight);
		resolver.GetFormatterWithVerify<LoadingData.EGroupType>().Serialize(ref writer, value.GroupType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.GroupArg, options);
		writer.Write(value.ShowRate);
	}

	public LoadingData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LoadingData loadingData = new LoadingData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				loadingData.Index = reader.ReadInt32();
				break;
			case 1:
				loadingData.Category = resolver.GetFormatterWithVerify<LoadingData.ECategory>().Deserialize(ref reader, options);
				break;
			case 2:
				loadingData.Argument = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 3:
				loadingData.HeroCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				loadingData.HeroPosOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 5:
				loadingData.HeroWidth = reader.ReadInt32();
				break;
			case 6:
				loadingData.HeroHeight = reader.ReadInt32();
				break;
			case 7:
				loadingData.GroupType = resolver.GetFormatterWithVerify<LoadingData.EGroupType>().Deserialize(ref reader, options);
				break;
			case 8:
				loadingData.GroupArg = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				loadingData.ShowRate = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return loadingData;
	}
}
