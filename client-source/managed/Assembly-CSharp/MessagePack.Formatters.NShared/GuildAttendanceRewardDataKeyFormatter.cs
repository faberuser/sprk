using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildAttendanceRewardDataKeyFormatter : IMessagePackFormatter<GuildAttendanceRewardDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildAttendanceRewardDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		writer.Write(value.Day);
	}

	public GuildAttendanceRewardDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int index = 0;
		int day = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				index = reader.ReadInt32();
				break;
			case 1:
				day = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		GuildAttendanceRewardDataKey result = new GuildAttendanceRewardDataKey(index, day);
		reader.Depth--;
		return result;
	}
}
