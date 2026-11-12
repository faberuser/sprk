using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class UpgradeBookItemDataContainer_TagGradeKeyFormatter : IMessagePackFormatter<UpgradeBookItemDataContainer.TagGradeKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, UpgradeBookItemDataContainer.TagGradeKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.Tag, options);
		writer.Write(value.Grade);
	}

	public UpgradeBookItemDataContainer.TagGradeKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureTagType tag = CreatureTagType.None;
		int grade = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tag = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 1:
				grade = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		UpgradeBookItemDataContainer.TagGradeKey result = new UpgradeBookItemDataContainer.TagGradeKey(tag, grade);
		reader.Depth--;
		return result;
	}
}
