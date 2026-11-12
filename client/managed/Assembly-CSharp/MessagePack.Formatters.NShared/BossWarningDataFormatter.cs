using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BossWarningDataFormatter : IMessagePackFormatter<BossWarningData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BossWarningData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(16);
		writer.Write(value.Index);
		writer.Write(value.SkillIndex);
		resolver.GetFormatterWithVerify<BossWarningData.EType>().Serialize(ref writer, value.WarningTargetType, options);
		resolver.GetFormatterWithVerify<BossWarningData.EWarningType>().Serialize(ref writer, value.SkillWarningType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SkillWarningOption, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffStartWarningType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffStartWarningOption, options);
		writer.Write(value.BuffStartStackCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffCrashWarningType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffCrashWarningOption, options);
		writer.Write(value.BuffCrashStackCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EffectType, options);
		writer.Write(value.DelayTime);
		writer.Write(value.DurationTime);
		writer.Write(value.Priority);
		writer.Write(value.CantCancel);
	}

	public BossWarningData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		BossWarningData bossWarningData = new BossWarningData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				bossWarningData.Index = reader.ReadInt32();
				break;
			case 1:
				bossWarningData.SkillIndex = reader.ReadInt32();
				break;
			case 2:
				bossWarningData.WarningTargetType = resolver.GetFormatterWithVerify<BossWarningData.EType>().Deserialize(ref reader, options);
				break;
			case 3:
				bossWarningData.SkillWarningType = resolver.GetFormatterWithVerify<BossWarningData.EWarningType>().Deserialize(ref reader, options);
				break;
			case 4:
				bossWarningData.SkillWarningOption = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				bossWarningData.BuffStartWarningType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				bossWarningData.BuffStartWarningOption = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				bossWarningData.BuffStartStackCount = reader.ReadInt32();
				break;
			case 8:
				bossWarningData.BuffCrashWarningType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				bossWarningData.BuffCrashWarningOption = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				bossWarningData.BuffCrashStackCount = reader.ReadInt32();
				break;
			case 11:
				bossWarningData.EffectType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				bossWarningData.DelayTime = reader.ReadSingle();
				break;
			case 13:
				bossWarningData.DurationTime = reader.ReadSingle();
				break;
			case 14:
				bossWarningData.Priority = reader.ReadInt32();
				break;
			case 15:
				bossWarningData.CantCancel = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return bossWarningData;
	}
}
