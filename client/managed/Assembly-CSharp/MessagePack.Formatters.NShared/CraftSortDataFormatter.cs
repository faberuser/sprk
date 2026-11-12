using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CraftSortDataFormatter : IMessagePackFormatter<CraftSortData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CraftSortData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<CraftSortCategory>().Serialize(ref writer, value.Category, options);
		writer.Write(value.Value);
		writer.Write(value.BackGroundIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StringKey, options);
	}

	public CraftSortData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CraftSortData craftSortData = new CraftSortData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				craftSortData.Category = resolver.GetFormatterWithVerify<CraftSortCategory>().Deserialize(ref reader, options);
				break;
			case 1:
				craftSortData.Value = reader.ReadInt32();
				break;
			case 2:
				craftSortData.BackGroundIndex = reader.ReadInt32();
				break;
			case 3:
				craftSortData.StringKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return craftSortData;
	}
}
