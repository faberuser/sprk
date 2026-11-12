using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildSkillLevelKey2Formatter : IMessagePackFormatter<GuildSkillLevelKey2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildSkillLevelKey2 value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.SkillIndex);
		writer.Write(value.Level);
	}

	public GuildSkillLevelKey2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int skillIndex = 0;
		int level = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				skillIndex = reader.ReadInt32();
				break;
			case 1:
				level = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		GuildSkillLevelKey2 result = new GuildSkillLevelKey2(skillIndex, level);
		reader.Depth--;
		return result;
	}
}
