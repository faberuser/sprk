using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureVisualDataFormatter : IMessagePackFormatter<CreatureVisualData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureVisualData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(16);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		writer.Write(value.HideHpBar);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OnSpawn, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OnBossIntro, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OnBattleStart, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OnBattleEnd, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OnDead, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DamageColor, options);
		writer.Write(value.IgnoreFlip);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ModelPrefix, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NamePrefixCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescriptionCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossIntroReplaceNameCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossIntroReplaceNamePrefixCode, options);
		writer.Write(value.CanSkipBossIntro);
	}

	public CreatureVisualData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureVisualData creatureVisualData = new CreatureVisualData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureVisualData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				creatureVisualData.HideHpBar = reader.ReadBoolean();
				break;
			case 2:
				creatureVisualData.OnSpawn = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				creatureVisualData.OnBossIntro = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				creatureVisualData.OnBattleStart = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				creatureVisualData.OnBattleEnd = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				creatureVisualData.OnDead = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				creatureVisualData.DamageColor = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				creatureVisualData.IgnoreFlip = reader.ReadBoolean();
				break;
			case 9:
				creatureVisualData.ModelPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				creatureVisualData.NameCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				creatureVisualData.NamePrefixCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				creatureVisualData.DescriptionCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				creatureVisualData.BossIntroReplaceNameCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				creatureVisualData.BossIntroReplaceNamePrefixCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				creatureVisualData.CanSkipBossIntro = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureVisualData;
	}
}
