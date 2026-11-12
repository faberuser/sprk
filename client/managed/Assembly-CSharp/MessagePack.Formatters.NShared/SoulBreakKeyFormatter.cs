using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulBreakKeyFormatter : IMessagePackFormatter<SoulBreakKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulBreakKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		writer.Write(value.Grade);
		writer.Write(value.ReinforceLevel);
	}

	public SoulBreakKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureTagType tagType = CreatureTagType.None;
		int grade = 0;
		int reinforceLevel = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 1:
				grade = reader.ReadInt32();
				break;
			case 2:
				reinforceLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		SoulBreakKey result = new SoulBreakKey(tagType, grade, reinforceLevel);
		reader.Depth--;
		return result;
	}
}
