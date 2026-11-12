using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RuneItemBreakDataFormatter : IMessagePackFormatter<RuneItemBreakData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RuneItemBreakData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(13);
		writer.Write(value.Grade);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode1, options);
		writer.Write(value.MinItemCount1);
		writer.Write(value.MaxItemCount1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode2, options);
		writer.Write(value.MinItemCount2);
		writer.Write(value.MaxItemCount2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode3, options);
		writer.Write(value.MinItemCount3);
		writer.Write(value.MaxItemCount3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode4, options);
		writer.Write(value.MinItemCount4);
		writer.Write(value.MaxItemCount4);
	}

	public RuneItemBreakData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RuneItemBreakData runeItemBreakData = new RuneItemBreakData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				runeItemBreakData.Grade = reader.ReadInt32();
				break;
			case 1:
				runeItemBreakData.ItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				runeItemBreakData.MinItemCount1 = reader.ReadInt32();
				break;
			case 3:
				runeItemBreakData.MaxItemCount1 = reader.ReadInt32();
				break;
			case 4:
				runeItemBreakData.ItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				runeItemBreakData.MinItemCount2 = reader.ReadInt32();
				break;
			case 6:
				runeItemBreakData.MaxItemCount2 = reader.ReadInt32();
				break;
			case 7:
				runeItemBreakData.ItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				runeItemBreakData.MinItemCount3 = reader.ReadInt32();
				break;
			case 9:
				runeItemBreakData.MaxItemCount3 = reader.ReadInt32();
				break;
			case 10:
				runeItemBreakData.ItemCode4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				runeItemBreakData.MinItemCount4 = reader.ReadInt32();
				break;
			case 12:
				runeItemBreakData.MaxItemCount4 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return runeItemBreakData;
	}
}
