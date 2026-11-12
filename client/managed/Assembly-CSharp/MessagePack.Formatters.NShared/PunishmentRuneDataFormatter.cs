using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRuneDataFormatter : IMessagePackFormatter<PunishmentRuneData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRuneData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(24);
		writer.Write(value.PunishmentRuneIndex);
		resolver.GetFormatterWithVerify<RuneSlotType[]>().Serialize(ref writer, value.SlotTypes, options);
		resolver.GetFormatterWithVerify<RuneColorType>().Serialize(ref writer, value.ColorType, options);
		writer.Write(value.VoidRuneIndex);
		writer.Write(value.LegendRuneIndex);
		writer.Write(value.ApostleBloodIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RuneTags, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.GaugePerRune, options);
		writer.Write(value.MaxGauge);
		writer.Write(value.MainOptionIndex);
		writer.Write(value.ExtraSkillIndex1);
		writer.Write(value.ExtraSkillValue1Min);
		writer.Write(value.ExtraSkillValue1Max);
		writer.Write(value.ExtraSkill1Step);
		writer.Write(value.ExtraSkillIndex2);
		writer.Write(value.ExtraSkillValue2Min);
		writer.Write(value.ExtraSkillValue2Max);
		writer.Write(value.ExtraSkill2Step);
		writer.Write(value.ExtraSkillIndex3);
		writer.Write(value.ExtraSkillValue3Min);
		writer.Write(value.ExtraSkillValue3Max);
		writer.Write(value.ExtraSkill3Step);
		writer.Write(value.ExtraSkillFraction);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.GuideSkillCode, options);
	}

	public PunishmentRuneData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PunishmentRuneData punishmentRuneData = new PunishmentRuneData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentRuneData.PunishmentRuneIndex = reader.ReadInt32();
				break;
			case 1:
				punishmentRuneData.SlotTypes = resolver.GetFormatterWithVerify<RuneSlotType[]>().Deserialize(ref reader, options);
				break;
			case 2:
				punishmentRuneData.ColorType = resolver.GetFormatterWithVerify<RuneColorType>().Deserialize(ref reader, options);
				break;
			case 3:
				punishmentRuneData.VoidRuneIndex = reader.ReadInt32();
				break;
			case 4:
				punishmentRuneData.LegendRuneIndex = reader.ReadInt32();
				break;
			case 5:
				punishmentRuneData.ApostleBloodIndex = reader.ReadInt32();
				break;
			case 6:
				punishmentRuneData.RuneTags = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				punishmentRuneData.GaugePerRune = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 8:
				punishmentRuneData.MaxGauge = reader.ReadInt32();
				break;
			case 9:
				punishmentRuneData.MainOptionIndex = reader.ReadInt32();
				break;
			case 10:
				punishmentRuneData.ExtraSkillIndex1 = reader.ReadInt32();
				break;
			case 11:
				punishmentRuneData.ExtraSkillValue1Min = reader.ReadInt32();
				break;
			case 12:
				punishmentRuneData.ExtraSkillValue1Max = reader.ReadInt32();
				break;
			case 13:
				punishmentRuneData.ExtraSkill1Step = reader.ReadInt32();
				break;
			case 14:
				punishmentRuneData.ExtraSkillIndex2 = reader.ReadInt32();
				break;
			case 15:
				punishmentRuneData.ExtraSkillValue2Min = reader.ReadInt32();
				break;
			case 16:
				punishmentRuneData.ExtraSkillValue2Max = reader.ReadInt32();
				break;
			case 17:
				punishmentRuneData.ExtraSkill2Step = reader.ReadInt32();
				break;
			case 18:
				punishmentRuneData.ExtraSkillIndex3 = reader.ReadInt32();
				break;
			case 19:
				punishmentRuneData.ExtraSkillValue3Min = reader.ReadInt32();
				break;
			case 20:
				punishmentRuneData.ExtraSkillValue3Max = reader.ReadInt32();
				break;
			case 21:
				punishmentRuneData.ExtraSkill3Step = reader.ReadInt32();
				break;
			case 22:
				punishmentRuneData.ExtraSkillFraction = reader.ReadInt32();
				break;
			case 23:
				punishmentRuneData.GuideSkillCode = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return punishmentRuneData;
	}
}
