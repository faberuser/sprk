using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HonoraryChamberMenuDataFormatter : IMessagePackFormatter<HonoraryChamberMenuData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HonoraryChamberMenuData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		writer.Write(value.ParentIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<RecordsOfHonorContentType>().Serialize(ref writer, value.ContentsType, options);
	}

	public HonoraryChamberMenuData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HonoraryChamberMenuData honoraryChamberMenuData = new HonoraryChamberMenuData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				honoraryChamberMenuData.Index = reader.ReadInt32();
				break;
			case 1:
				honoraryChamberMenuData.ParentIndex = reader.ReadInt32();
				break;
			case 2:
				honoraryChamberMenuData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				honoraryChamberMenuData.ContentsType = resolver.GetFormatterWithVerify<RecordsOfHonorContentType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return honoraryChamberMenuData;
	}
}
