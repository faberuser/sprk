using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WantedPathDataFormatter : IMessagePackFormatter<WantedPathData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WantedPathData value, MessagePackSerializerOptions options)
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
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DungeonIndices, options);
	}

	public WantedPathData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		WantedPathData wantedPathData = new WantedPathData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				wantedPathData.Index = reader.ReadInt32();
				break;
			case 1:
				wantedPathData.ChapterIndex = reader.ReadInt32();
				break;
			case 2:
				wantedPathData.DungeonIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return wantedPathData;
	}
}
