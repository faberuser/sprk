using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AttendanceRewardUrlDataFormatter : IMessagePackFormatter<AttendanceRewardUrlData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AttendanceRewardUrlData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		writer.Write(value.Day);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Language, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Url, options);
	}

	public AttendanceRewardUrlData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AttendanceRewardUrlData attendanceRewardUrlData = new AttendanceRewardUrlData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				attendanceRewardUrlData.Index = reader.ReadInt32();
				break;
			case 1:
				attendanceRewardUrlData.Day = reader.ReadInt32();
				break;
			case 2:
				attendanceRewardUrlData.Language = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				attendanceRewardUrlData.Url = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return attendanceRewardUrlData;
	}
}
