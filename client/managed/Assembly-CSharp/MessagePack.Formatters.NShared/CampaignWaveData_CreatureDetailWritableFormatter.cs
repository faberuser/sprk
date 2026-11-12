using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignWaveData_CreatureDetailWritableFormatter : IMessagePackFormatter<CampaignWaveData.CreatureDetailWritable>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignWaveData.CreatureDetailWritable value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(19);
		writer.Write(value._Index);
		writer.Write(value._Star);
		writer.Write(value._Level);
		writer.Write(value._Tier);
		resolver.GetFormatterWithVerify<MonsterGradeType>().Serialize(ref writer, value._Grade, options);
		resolver.GetFormatterWithVerify<CampaignWaveData.EWaveFlag>().Serialize(ref writer, value._WaveFlag, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value._VisualInfo, options);
		writer.Write(value._OffsetMx);
		writer.Write(value._OffsetMz);
		writer.Write(value._DelayMs);
		writer.Write(value._ScaleR);
		writer.Write(value._HeightMm);
		writer.Write(value._SkillOpen);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value._AddExtraSkill, options);
		writer.Write(value._WeaponIndex);
		writer.Write(value._GroupIndex);
		writer.Write(value._ForceMinHp);
		writer.Write(value._IsBoss);
		writer.Write(value._WaveSlotIndex);
	}

	public CampaignWaveData.CreatureDetailWritable Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignWaveData.CreatureDetailWritable creatureDetailWritable = new CampaignWaveData.CreatureDetailWritable();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureDetailWritable._Index = reader.ReadInt32();
				break;
			case 1:
				creatureDetailWritable._Star = reader.ReadInt32();
				break;
			case 2:
				creatureDetailWritable._Level = reader.ReadInt32();
				break;
			case 3:
				creatureDetailWritable._Tier = reader.ReadInt32();
				break;
			case 4:
				creatureDetailWritable._Grade = resolver.GetFormatterWithVerify<MonsterGradeType>().Deserialize(ref reader, options);
				break;
			case 5:
				creatureDetailWritable._WaveFlag = resolver.GetFormatterWithVerify<CampaignWaveData.EWaveFlag>().Deserialize(ref reader, options);
				break;
			case 6:
				creatureDetailWritable._VisualInfo = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				creatureDetailWritable._OffsetMx = reader.ReadInt32();
				break;
			case 8:
				creatureDetailWritable._OffsetMz = reader.ReadInt32();
				break;
			case 9:
				creatureDetailWritable._DelayMs = reader.ReadInt32();
				break;
			case 10:
				creatureDetailWritable._ScaleR = reader.ReadInt32();
				break;
			case 11:
				creatureDetailWritable._HeightMm = reader.ReadInt32();
				break;
			case 12:
				creatureDetailWritable._SkillOpen = reader.ReadInt32();
				break;
			case 13:
				creatureDetailWritable._AddExtraSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 14:
				creatureDetailWritable._WeaponIndex = reader.ReadInt32();
				break;
			case 15:
				creatureDetailWritable._GroupIndex = reader.ReadInt32();
				break;
			case 16:
				creatureDetailWritable._ForceMinHp = reader.ReadInt64();
				break;
			case 17:
				creatureDetailWritable._IsBoss = reader.ReadBoolean();
				break;
			case 18:
				creatureDetailWritable._WaveSlotIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureDetailWritable;
	}
}
