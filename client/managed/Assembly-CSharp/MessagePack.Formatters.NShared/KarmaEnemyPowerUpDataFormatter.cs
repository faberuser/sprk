using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaEnemyPowerUpDataFormatter : IMessagePackFormatter<KarmaEnemyPowerUpData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaEnemyPowerUpData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.SkillIndex);
		writer.Write(value.PunishmentGroupIndex);
	}

	public KarmaEnemyPowerUpData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		KarmaEnemyPowerUpData karmaEnemyPowerUpData = new KarmaEnemyPowerUpData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				karmaEnemyPowerUpData.SkillIndex = reader.ReadInt32();
				break;
			case 1:
				karmaEnemyPowerUpData.PunishmentGroupIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return karmaEnemyPowerUpData;
	}
}
