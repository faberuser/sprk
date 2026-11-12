using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AttendanceRewardInfoDataFormatter : IMessagePackFormatter<AttendanceRewardInfoData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AttendanceRewardInfoData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		writer.Write(value.Day);
		writer.Write(value.No);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardTitleKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardDescKey, options);
	}

	public AttendanceRewardInfoData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AttendanceRewardInfoData attendanceRewardInfoData = new AttendanceRewardInfoData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				attendanceRewardInfoData.Index = reader.ReadInt32();
				break;
			case 1:
				attendanceRewardInfoData.Day = reader.ReadInt32();
				break;
			case 2:
				attendanceRewardInfoData.No = reader.ReadInt32();
				break;
			case 3:
				attendanceRewardInfoData.RewardTitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				attendanceRewardInfoData.RewardDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return attendanceRewardInfoData;
	}
}
