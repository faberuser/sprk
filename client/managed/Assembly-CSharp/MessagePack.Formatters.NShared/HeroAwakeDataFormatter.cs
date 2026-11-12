using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroAwakeDataFormatter : IMessagePackFormatter<HeroAwakeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroAwakeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(20);
		writer.Write(value.HeroIndex);
		writer.Write(value.AwakeChapter);
		writer.Write(value.AwakeDungeon1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AwakeMaterialItemCode1, options);
		writer.Write(value.AwakeDungeon2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AwakeMaterialItemCode2, options);
		writer.Write(value.AwakeDungeon3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AwakeMaterialItemCode3, options);
		writer.Write(value.AwakeDungeon4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AwakeMaterialItemCode4, options);
		writer.Write(value.TranscendDungeon0);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TranscendMaterialItemCode0, options);
		writer.Write(value.TranscendDungeon1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TranscendMaterialItemCode1, options);
		writer.Write(value.TranscendDungeon2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TranscendMaterialItemCode2, options);
		writer.Write(value.TranscendDungeon3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TranscendMaterialItemCode3, options);
		writer.Write(value.TranscendDungeon4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TranscendMaterialItemCode4, options);
	}

	public HeroAwakeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroAwakeData heroAwakeData = new HeroAwakeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroAwakeData.HeroIndex = reader.ReadInt32();
				break;
			case 1:
				heroAwakeData.AwakeChapter = reader.ReadInt32();
				break;
			case 2:
				heroAwakeData.AwakeDungeon1 = reader.ReadInt32();
				break;
			case 3:
				heroAwakeData.AwakeMaterialItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				heroAwakeData.AwakeDungeon2 = reader.ReadInt32();
				break;
			case 5:
				heroAwakeData.AwakeMaterialItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				heroAwakeData.AwakeDungeon3 = reader.ReadInt32();
				break;
			case 7:
				heroAwakeData.AwakeMaterialItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				heroAwakeData.AwakeDungeon4 = reader.ReadInt32();
				break;
			case 9:
				heroAwakeData.AwakeMaterialItemCode4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				heroAwakeData.TranscendDungeon0 = reader.ReadInt32();
				break;
			case 11:
				heroAwakeData.TranscendMaterialItemCode0 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				heroAwakeData.TranscendDungeon1 = reader.ReadInt32();
				break;
			case 13:
				heroAwakeData.TranscendMaterialItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				heroAwakeData.TranscendDungeon2 = reader.ReadInt32();
				break;
			case 15:
				heroAwakeData.TranscendMaterialItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				heroAwakeData.TranscendDungeon3 = reader.ReadInt32();
				break;
			case 17:
				heroAwakeData.TranscendMaterialItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				heroAwakeData.TranscendDungeon4 = reader.ReadInt32();
				break;
			case 19:
				heroAwakeData.TranscendMaterialItemCode4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroAwakeData;
	}
}
