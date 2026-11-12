using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipUpgradeStatDataFormatter : IMessagePackFormatter<EquipUpgradeStatData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipUpgradeStatData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(27);
		writer.Write(value.Star);
		writer.Write(value.DetailIndex);
		writer.Write(value.Level);
		writer.Write(value.PhysicalAttack);
		writer.Write(value.MagicalAttack);
		writer.Write(value.MaxHp);
		writer.Write(value.PhysicalDefense);
		writer.Write(value.MagicalDefense);
		writer.Write(value.AddPhysicalAttackR);
		writer.Write(value.AddMagicalAttackR);
		writer.Write(value.AddMaxHpR);
		writer.Write(value.AddPhysicalDefenseR);
		writer.Write(value.AddMagicalDefenseR);
		writer.Write(value.AddValuePhysicalAttack);
		writer.Write(value.AddValueMagicalAttack);
		writer.Write(value.AddValueMaxHp);
		writer.Write(value.AddValuePhysicalDefense);
		writer.Write(value.AddValueMagicalDefense);
		writer.Write(value.AddValueAddPhysicalAttackR);
		writer.Write(value.AddValueAddMagicalAttackR);
		writer.Write(value.AddValueAddMaxHpR);
		writer.Write(value.AddValueAddPhysicalDefenseR);
		writer.Write(value.AddValueAddMagicalDefenseR);
		writer.Write(value.AddValuePhysicalCriticalChance);
		writer.Write(value.AddValueMagicalCriticalChance);
		writer.Write(value.AddValuePhysicalToughness);
		writer.Write(value.AddValueMagicalToughness);
	}

	public EquipUpgradeStatData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EquipUpgradeStatData equipUpgradeStatData = new EquipUpgradeStatData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipUpgradeStatData.Star = reader.ReadInt32();
				break;
			case 1:
				equipUpgradeStatData.DetailIndex = reader.ReadInt32();
				break;
			case 2:
				equipUpgradeStatData.Level = reader.ReadInt32();
				break;
			case 3:
				equipUpgradeStatData.PhysicalAttack = reader.ReadInt32();
				break;
			case 4:
				equipUpgradeStatData.MagicalAttack = reader.ReadInt32();
				break;
			case 5:
				equipUpgradeStatData.MaxHp = reader.ReadInt32();
				break;
			case 6:
				equipUpgradeStatData.PhysicalDefense = reader.ReadInt32();
				break;
			case 7:
				equipUpgradeStatData.MagicalDefense = reader.ReadInt32();
				break;
			case 8:
				equipUpgradeStatData.AddPhysicalAttackR = reader.ReadInt32();
				break;
			case 9:
				equipUpgradeStatData.AddMagicalAttackR = reader.ReadInt32();
				break;
			case 10:
				equipUpgradeStatData.AddMaxHpR = reader.ReadInt32();
				break;
			case 11:
				equipUpgradeStatData.AddPhysicalDefenseR = reader.ReadInt32();
				break;
			case 12:
				equipUpgradeStatData.AddMagicalDefenseR = reader.ReadInt32();
				break;
			case 13:
				equipUpgradeStatData.AddValuePhysicalAttack = reader.ReadInt32();
				break;
			case 14:
				equipUpgradeStatData.AddValueMagicalAttack = reader.ReadInt32();
				break;
			case 15:
				equipUpgradeStatData.AddValueMaxHp = reader.ReadInt32();
				break;
			case 16:
				equipUpgradeStatData.AddValuePhysicalDefense = reader.ReadInt32();
				break;
			case 17:
				equipUpgradeStatData.AddValueMagicalDefense = reader.ReadInt32();
				break;
			case 18:
				equipUpgradeStatData.AddValueAddPhysicalAttackR = reader.ReadInt32();
				break;
			case 19:
				equipUpgradeStatData.AddValueAddMagicalAttackR = reader.ReadInt32();
				break;
			case 20:
				equipUpgradeStatData.AddValueAddMaxHpR = reader.ReadInt32();
				break;
			case 21:
				equipUpgradeStatData.AddValueAddPhysicalDefenseR = reader.ReadInt32();
				break;
			case 22:
				equipUpgradeStatData.AddValueAddMagicalDefenseR = reader.ReadInt32();
				break;
			case 23:
				equipUpgradeStatData.AddValuePhysicalCriticalChance = reader.ReadInt32();
				break;
			case 24:
				equipUpgradeStatData.AddValueMagicalCriticalChance = reader.ReadInt32();
				break;
			case 25:
				equipUpgradeStatData.AddValuePhysicalToughness = reader.ReadInt32();
				break;
			case 26:
				equipUpgradeStatData.AddValueMagicalToughness = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipUpgradeStatData;
	}
}
