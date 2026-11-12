using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildSkillLevelDataFormatter : IMessagePackFormatter<GuildSkillLevelData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildSkillLevelData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.GuildSkillIndex);
		writer.Write(value.SkillIndex);
		writer.Write(value.GuildSkillLevel);
		writer.Write(value.ActivityPoint);
		writer.Write(value.TotalActivityPoint);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.Values, options);
		writer.Write(value.SuppressPoint);
		writer.Write(value.TotalSuppressPoint);
		writer.Write(value.TrainingCenterLevel);
	}

	public GuildSkillLevelData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GuildSkillLevelData guildSkillLevelData = new GuildSkillLevelData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildSkillLevelData.GuildSkillIndex = reader.ReadInt32();
				break;
			case 1:
				guildSkillLevelData.SkillIndex = reader.ReadInt32();
				break;
			case 2:
				guildSkillLevelData.GuildSkillLevel = reader.ReadByte();
				break;
			case 3:
				guildSkillLevelData.ActivityPoint = reader.ReadInt32();
				break;
			case 4:
				guildSkillLevelData.TotalActivityPoint = reader.ReadInt32();
				break;
			case 5:
				guildSkillLevelData.Values = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 6:
				guildSkillLevelData.SuppressPoint = reader.ReadInt32();
				break;
			case 7:
				guildSkillLevelData.TotalSuppressPoint = reader.ReadInt32();
				break;
			case 8:
				guildSkillLevelData.TrainingCenterLevel = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildSkillLevelData;
	}
}
