using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaEnemyUnlockSkillDataFormatter : IMessagePackFormatter<KarmaEnemyUnlockSkillData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaEnemyUnlockSkillData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.TriggerLevel);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.UnlockSkills, options);
	}

	public KarmaEnemyUnlockSkillData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		KarmaEnemyUnlockSkillData karmaEnemyUnlockSkillData = new KarmaEnemyUnlockSkillData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				karmaEnemyUnlockSkillData.PunishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				karmaEnemyUnlockSkillData.TriggerLevel = reader.ReadInt32();
				break;
			case 2:
				karmaEnemyUnlockSkillData.UnlockSkills = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return karmaEnemyUnlockSkillData;
	}
}
