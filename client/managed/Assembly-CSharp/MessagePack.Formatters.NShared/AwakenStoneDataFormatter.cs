using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AwakenStoneDataFormatter : IMessagePackFormatter<AwakenStoneData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AwakenStoneData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<ItemType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.AwakenStar);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ItemIndices, options);
		writer.Write(value.IsAllowNPC);
	}

	public AwakenStoneData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AwakenStoneData awakenStoneData = new AwakenStoneData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				awakenStoneData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				awakenStoneData.Type = resolver.GetFormatterWithVerify<ItemType>().Deserialize(ref reader, options);
				break;
			case 2:
				awakenStoneData.AwakenStar = reader.ReadInt32();
				break;
			case 3:
				awakenStoneData.ItemIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 4:
				awakenStoneData.IsAllowNPC = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return awakenStoneData;
	}
}
