using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class VideoDataFormatter : IMessagePackFormatter<VideoData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, VideoData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Path, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
	}

	public VideoData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		VideoData videoData = new VideoData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				videoData.Index = reader.ReadInt32();
				break;
			case 1:
				videoData.Path = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				videoData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return videoData;
	}
}
