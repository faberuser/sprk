using System.Collections.Generic;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulBreakDataFormatter : IMessagePackFormatter<SoulBreakData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulBreakData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		writer.Write(value.Grade);
		writer.Write(value.ReinforceLevel);
		resolver.GetFormatterWithVerify<List<SoulBreakData.ItemCodeMinMaxData>>().Serialize(ref writer, value.ItemMinMaxList, options);
	}

	public SoulBreakData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulBreakData soulBreakData = new SoulBreakData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulBreakData.TagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 1:
				soulBreakData.Grade = reader.ReadInt32();
				break;
			case 2:
				soulBreakData.ReinforceLevel = reader.ReadInt32();
				break;
			case 3:
				soulBreakData.ItemMinMaxList = resolver.GetFormatterWithVerify<List<SoulBreakData.ItemCodeMinMaxData>>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulBreakData;
	}
}
