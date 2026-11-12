using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureLevelStatData2Formatter : IMessagePackFormatter<CreatureLevelStatData2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureLevelStatData2 value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.CreatureIndex);
		writer.Write(value.PhysicalAttackBase);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.PhysicalAttackFactor, options);
		writer.Write(value.MagicalAttackBase);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.MagicalAttackFactor, options);
		writer.Write(value.MaxHpBase);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.MaxHpFactor, options);
		writer.Write(value.PhysicalDefenseBase);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.PhysicalDefenseFactor, options);
		writer.Write(value.MagicalDefenseBase);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.MagicalDefenseFactor, options);
	}

	public CreatureLevelStatData2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureLevelStatData2 creatureLevelStatData = new CreatureLevelStatData2();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureLevelStatData.CreatureIndex = reader.ReadInt32();
				break;
			case 1:
				creatureLevelStatData.PhysicalAttackBase = reader.ReadInt64();
				break;
			case 2:
				creatureLevelStatData.PhysicalAttackFactor = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				creatureLevelStatData.MagicalAttackBase = reader.ReadInt64();
				break;
			case 4:
				creatureLevelStatData.MagicalAttackFactor = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				creatureLevelStatData.MaxHpBase = reader.ReadInt64();
				break;
			case 6:
				creatureLevelStatData.MaxHpFactor = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 7:
				creatureLevelStatData.PhysicalDefenseBase = reader.ReadInt64();
				break;
			case 8:
				creatureLevelStatData.PhysicalDefenseFactor = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 9:
				creatureLevelStatData.MagicalDefenseBase = reader.ReadInt64();
				break;
			case 10:
				creatureLevelStatData.MagicalDefenseFactor = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureLevelStatData;
	}
}
