using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClassBuffDataBaseFormatter : IMessagePackFormatter<ClassBuffDataBase>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClassBuffDataBase value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.CreatureType);
		writer.Write(value.ClassBuffIndex);
		writer.Write(value.SkillIndex);
	}

	public ClassBuffDataBase Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		ClassBuffDataBase classBuffDataBase = new ClassBuffDataBase();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				classBuffDataBase.CreatureType = reader.ReadInt32();
				break;
			case 1:
				classBuffDataBase.ClassBuffIndex = reader.ReadInt32();
				break;
			case 2:
				classBuffDataBase.SkillIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return classBuffDataBase;
	}
}
