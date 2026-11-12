using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CodecSupportListDataFormatter : IMessagePackFormatter<CodecSupportListData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CodecSupportListData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.VideoType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CodecType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OS, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OSVersion, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ModelManufacturer, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TargetSpec, options);
	}

	public CodecSupportListData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CodecSupportListData codecSupportListData = new CodecSupportListData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				codecSupportListData.Index = reader.ReadInt32();
				break;
			case 1:
				codecSupportListData.VideoType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				codecSupportListData.CodecType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				codecSupportListData.OS = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				codecSupportListData.OSVersion = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				codecSupportListData.ModelManufacturer = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				codecSupportListData.TargetSpec = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return codecSupportListData;
	}
}
