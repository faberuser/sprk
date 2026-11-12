using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildSkillDataBaseFormatter : IMessagePackFormatter<GuildSkillDataBase>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildSkillDataBase value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.GuildSkillIndex);
		writer.Write(value.SkillIndex);
		writer.Write(value.GuildSkillLevel);
	}

	public GuildSkillDataBase Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		GuildSkillDataBase guildSkillDataBase = new GuildSkillDataBase();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				guildSkillDataBase.GuildSkillIndex = reader.ReadInt32();
				break;
			case 1:
				guildSkillDataBase.SkillIndex = reader.ReadInt32();
				break;
			case 2:
				guildSkillDataBase.GuildSkillLevel = reader.ReadByte();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return guildSkillDataBase;
	}
}
