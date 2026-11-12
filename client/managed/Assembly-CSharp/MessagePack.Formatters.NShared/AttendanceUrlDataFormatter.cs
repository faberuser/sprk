using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AttendanceUrlDataFormatter : IMessagePackFormatter<AttendanceUrlData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AttendanceUrlData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Language, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Url, options);
	}

	public AttendanceUrlData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AttendanceUrlData attendanceUrlData = new AttendanceUrlData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				attendanceUrlData.Index = reader.ReadInt32();
				break;
			case 1:
				attendanceUrlData.Language = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				attendanceUrlData.Url = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return attendanceUrlData;
	}
}
