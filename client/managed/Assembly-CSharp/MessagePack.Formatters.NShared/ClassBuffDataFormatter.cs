using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClassBuffDataFormatter : IMessagePackFormatter<ClassBuffData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClassBuffData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		writer.Write(value.CreatureType);
		writer.Write(value.ClassBuffIndex);
		writer.Write(value.SkillIndex);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		writer.Write(value.ClassBuffLevel);
		writer.Write(value.NeedPoint);
		writer.Write(value.NeedClassBuffIndex);
		writer.Write(value.NeedClassBuffLevel);
		writer.Write(value.NeedHeroCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ClassBuffIconPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffOptionTitleName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffOptionTitleDesc, options);
		writer.Write(value.StatUsePercentage);
		writer.Write(value.Grade);
	}

	public ClassBuffData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ClassBuffData classBuffData = new ClassBuffData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				classBuffData.CreatureType = reader.ReadInt32();
				break;
			case 1:
				classBuffData.ClassBuffIndex = reader.ReadInt32();
				break;
			case 2:
				classBuffData.SkillIndex = reader.ReadInt32();
				break;
			case 3:
				classBuffData.Index = reader.ReadInt32();
				break;
			case 4:
				classBuffData.TagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 5:
				classBuffData.ClassBuffLevel = reader.ReadInt32();
				break;
			case 6:
				classBuffData.NeedPoint = reader.ReadInt64();
				break;
			case 7:
				classBuffData.NeedClassBuffIndex = reader.ReadInt32();
				break;
			case 8:
				classBuffData.NeedClassBuffLevel = reader.ReadInt32();
				break;
			case 9:
				classBuffData.NeedHeroCount = reader.ReadInt32();
				break;
			case 10:
				classBuffData.ClassBuffIconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				classBuffData.BuffOptionTitleName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				classBuffData.BuffOptionTitleDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				classBuffData.StatUsePercentage = reader.ReadByte();
				break;
			case 14:
				classBuffData.Grade = reader.ReadByte();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return classBuffData;
	}
}
