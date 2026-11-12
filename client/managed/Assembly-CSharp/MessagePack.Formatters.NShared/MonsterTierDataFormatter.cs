using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MonsterTierDataFormatter : IMessagePackFormatter<MonsterTierData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MonsterTierData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(8);
		writer.Write(value.Index);
		writer.Write(value.PhysicalAttack);
		writer.Write(value.MagicalAttack);
		writer.Write(value.PhysicalDefense);
		writer.Write(value.MagicalDefense);
		writer.Write(value.MaxHp);
		writer.Write(value.MaxMp);
		writer.Write(value.AntiCcChance);
	}

	public MonsterTierData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		MonsterTierData monsterTierData = new MonsterTierData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				monsterTierData.Index = reader.ReadInt32();
				break;
			case 1:
				monsterTierData.PhysicalAttack = reader.ReadInt64();
				break;
			case 2:
				monsterTierData.MagicalAttack = reader.ReadInt64();
				break;
			case 3:
				monsterTierData.PhysicalDefense = reader.ReadInt64();
				break;
			case 4:
				monsterTierData.MagicalDefense = reader.ReadInt64();
				break;
			case 5:
				monsterTierData.MaxHp = reader.ReadInt64();
				break;
			case 6:
				monsterTierData.MaxMp = reader.ReadInt32();
				break;
			case 7:
				monsterTierData.AntiCcChance = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return monsterTierData;
	}
}
