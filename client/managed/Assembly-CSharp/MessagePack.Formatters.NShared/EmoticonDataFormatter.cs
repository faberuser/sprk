using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EmoticonDataFormatter : IMessagePackFormatter<EmoticonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EmoticonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Keyword, options);
		writer.Write(value.TabIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AssetPath, options);
	}

	public EmoticonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EmoticonData emoticonData = new EmoticonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				emoticonData.Index = reader.ReadInt32();
				break;
			case 1:
				emoticonData.Keyword = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				emoticonData.TabIndex = reader.ReadInt32();
				break;
			case 3:
				emoticonData.AssetPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return emoticonData;
	}
}
