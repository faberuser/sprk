using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EmoticonTabDataFormatter : IMessagePackFormatter<EmoticonTabData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EmoticonTabData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Tab, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AssetPath, options);
		writer.Write(value.ShowIndex);
	}

	public EmoticonTabData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EmoticonTabData emoticonTabData = new EmoticonTabData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				emoticonTabData.Index = reader.ReadInt32();
				break;
			case 1:
				emoticonTabData.Tab = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				emoticonTabData.AssetPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				emoticonTabData.ShowIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return emoticonTabData;
	}
}
