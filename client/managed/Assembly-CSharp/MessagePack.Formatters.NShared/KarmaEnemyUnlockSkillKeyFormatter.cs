using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaEnemyUnlockSkillKeyFormatter : IMessagePackFormatter<KarmaEnemyUnlockSkillKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaEnemyUnlockSkillKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.TriggerLevel);
	}

	public KarmaEnemyUnlockSkillKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int punishmentGroupIndex = 0;
		int triggerLevel = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				triggerLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		KarmaEnemyUnlockSkillKey result = new KarmaEnemyUnlockSkillKey(punishmentGroupIndex, triggerLevel);
		reader.Depth--;
		return result;
	}
}
