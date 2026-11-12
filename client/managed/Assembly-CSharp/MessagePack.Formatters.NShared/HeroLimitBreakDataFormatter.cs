using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroLimitBreakDataFormatter : IMessagePackFormatter<HeroLimitBreakData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroLimitBreakData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		writer.Write(value.HeroIndex);
		writer.Write(value.LimitBreakLevel);
		writer.Write(value.ReqLocalExp);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode1, options);
		writer.Write(value.MaterialItemCount1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode2, options);
		writer.Write(value.MaterialItemCount2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaterialItemCode3, options);
		writer.Write(value.MaterialItemCount3);
		writer.Write(value.GetTranscendSkillPoint);
	}

	public HeroLimitBreakData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroLimitBreakData heroLimitBreakData = new HeroLimitBreakData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroLimitBreakData.HeroIndex = reader.ReadInt32();
				break;
			case 1:
				heroLimitBreakData.LimitBreakLevel = reader.ReadInt32();
				break;
			case 2:
				heroLimitBreakData.ReqLocalExp = reader.ReadInt32();
				break;
			case 3:
				heroLimitBreakData.MaterialItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				heroLimitBreakData.MaterialItemCount1 = reader.ReadInt32();
				break;
			case 5:
				heroLimitBreakData.MaterialItemCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				heroLimitBreakData.MaterialItemCount2 = reader.ReadInt32();
				break;
			case 7:
				heroLimitBreakData.MaterialItemCode3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				heroLimitBreakData.MaterialItemCount3 = reader.ReadInt32();
				break;
			case 9:
				heroLimitBreakData.GetTranscendSkillPoint = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroLimitBreakData;
	}
}
