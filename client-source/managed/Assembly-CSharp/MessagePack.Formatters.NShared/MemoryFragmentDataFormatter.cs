using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MemoryFragmentDataFormatter : IMessagePackFormatter<MemoryFragmentData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MemoryFragmentData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenNpcCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortalPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonDesc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SelectDungeonName, options);
	}

	public MemoryFragmentData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MemoryFragmentData memoryFragmentData = new MemoryFragmentData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				memoryFragmentData.Index = reader.ReadInt32();
				break;
			case 1:
				memoryFragmentData.OpenNpcCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				memoryFragmentData.PortalPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				memoryFragmentData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				memoryFragmentData.DungeonName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				memoryFragmentData.DungeonDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				memoryFragmentData.SelectDungeonName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return memoryFragmentData;
	}
}
