using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DownloadControlDataFormatter : IMessagePackFormatter<DownloadControlData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DownloadControlData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		writer.Write(value.ChapterIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Categories, options);
	}

	public DownloadControlData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		DownloadControlData downloadControlData = new DownloadControlData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				downloadControlData.Index = reader.ReadInt32();
				break;
			case 1:
				downloadControlData.ChapterIndex = reader.ReadInt32();
				break;
			case 2:
				downloadControlData.Categories = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return downloadControlData;
	}
}
