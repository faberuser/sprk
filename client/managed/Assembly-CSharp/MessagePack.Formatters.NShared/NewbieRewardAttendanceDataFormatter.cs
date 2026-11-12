using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewbieRewardAttendanceDataFormatter : IMessagePackFormatter<NewbieRewardAttendanceData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewbieRewardAttendanceData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EAttendanceKeyword>().Serialize(ref writer, value.Keyword, options);
		resolver.GetFormatterWithVerify<NewbieRewardType>().Serialize(ref writer, value.TabType, options);
	}

	public NewbieRewardAttendanceData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewbieRewardAttendanceData newbieRewardAttendanceData = new NewbieRewardAttendanceData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newbieRewardAttendanceData.Index = reader.ReadInt32();
				break;
			case 1:
				newbieRewardAttendanceData.Keyword = resolver.GetFormatterWithVerify<EAttendanceKeyword>().Deserialize(ref reader, options);
				break;
			case 2:
				newbieRewardAttendanceData.TabType = resolver.GetFormatterWithVerify<NewbieRewardType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newbieRewardAttendanceData;
	}
}
