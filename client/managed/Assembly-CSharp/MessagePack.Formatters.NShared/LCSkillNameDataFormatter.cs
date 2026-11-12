using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LCSkillNameDataFormatter : IMessagePackFormatter<LCSkillNameData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LCSkillNameData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(31);
		writer.Write(value.Index);
		writer.Write(value.SkillIndex1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey1, options);
		writer.Write(value.SkillIndex2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey2, options);
		writer.Write(value.SkillIndex3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey3, options);
		writer.Write(value.SkillIndex4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey4, options);
		writer.Write(value.SkillIndex5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey5, options);
		writer.Write(value.SkillIndex6);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey6, options);
		writer.Write(value.SkillIndex7);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey7, options);
		writer.Write(value.SkillIndex8);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey8, options);
		writer.Write(value.SkillIndex9);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey9, options);
		writer.Write(value.SkillIndex10);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey10, options);
		writer.Write(value.SkillIndex11);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey11, options);
		writer.Write(value.SkillIndex12);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey12, options);
		writer.Write(value.SkillIndex13);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey13, options);
		writer.Write(value.SkillIndex14);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey14, options);
		writer.Write(value.SkillIndex15);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillNameKey15, options);
	}

	public LCSkillNameData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LCSkillNameData lCSkillNameData = new LCSkillNameData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				lCSkillNameData.Index = reader.ReadInt32();
				break;
			case 1:
				lCSkillNameData.SkillIndex1 = reader.ReadInt32();
				break;
			case 2:
				lCSkillNameData.SkillNameKey1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				lCSkillNameData.SkillIndex2 = reader.ReadInt32();
				break;
			case 4:
				lCSkillNameData.SkillNameKey2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				lCSkillNameData.SkillIndex3 = reader.ReadInt32();
				break;
			case 6:
				lCSkillNameData.SkillNameKey3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				lCSkillNameData.SkillIndex4 = reader.ReadInt32();
				break;
			case 8:
				lCSkillNameData.SkillNameKey4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				lCSkillNameData.SkillIndex5 = reader.ReadInt32();
				break;
			case 10:
				lCSkillNameData.SkillNameKey5 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				lCSkillNameData.SkillIndex6 = reader.ReadInt32();
				break;
			case 12:
				lCSkillNameData.SkillNameKey6 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				lCSkillNameData.SkillIndex7 = reader.ReadInt32();
				break;
			case 14:
				lCSkillNameData.SkillNameKey7 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				lCSkillNameData.SkillIndex8 = reader.ReadInt32();
				break;
			case 16:
				lCSkillNameData.SkillNameKey8 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				lCSkillNameData.SkillIndex9 = reader.ReadInt32();
				break;
			case 18:
				lCSkillNameData.SkillNameKey9 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				lCSkillNameData.SkillIndex10 = reader.ReadInt32();
				break;
			case 20:
				lCSkillNameData.SkillNameKey10 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 21:
				lCSkillNameData.SkillIndex11 = reader.ReadInt32();
				break;
			case 22:
				lCSkillNameData.SkillNameKey11 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 23:
				lCSkillNameData.SkillIndex12 = reader.ReadInt32();
				break;
			case 24:
				lCSkillNameData.SkillNameKey12 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 25:
				lCSkillNameData.SkillIndex13 = reader.ReadInt32();
				break;
			case 26:
				lCSkillNameData.SkillNameKey13 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 27:
				lCSkillNameData.SkillIndex14 = reader.ReadInt32();
				break;
			case 28:
				lCSkillNameData.SkillNameKey14 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 29:
				lCSkillNameData.SkillIndex15 = reader.ReadInt32();
				break;
			case 30:
				lCSkillNameData.SkillNameKey15 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return lCSkillNameData;
	}
}
