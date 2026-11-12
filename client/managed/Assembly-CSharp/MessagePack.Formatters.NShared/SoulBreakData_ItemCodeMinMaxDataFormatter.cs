using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulBreakData_ItemCodeMinMaxDataFormatter : IMessagePackFormatter<SoulBreakData.ItemCodeMinMaxData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulBreakData.ItemCodeMinMaxData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.MinItemCount);
		writer.Write(value.MaxItemCount);
	}

	public SoulBreakData.ItemCodeMinMaxData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulBreakData.ItemCodeMinMaxData itemCodeMinMaxData = new SoulBreakData.ItemCodeMinMaxData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemCodeMinMaxData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				itemCodeMinMaxData.MinItemCount = reader.ReadInt32();
				break;
			case 2:
				itemCodeMinMaxData.MaxItemCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return itemCodeMinMaxData;
	}
}
