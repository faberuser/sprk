using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AttendanceRewardInfoKeyFormatter : IMessagePackFormatter<AttendanceRewardInfoKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AttendanceRewardInfoKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		writer.Write(value.Day);
		writer.Write(value.No);
	}

	public AttendanceRewardInfoKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int index = 0;
		int day = 0;
		int no = 0;
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
			case 2:
				no = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		AttendanceRewardInfoKey result = new AttendanceRewardInfoKey(index, day, no);
		reader.Depth--;
		return result;
	}
}
