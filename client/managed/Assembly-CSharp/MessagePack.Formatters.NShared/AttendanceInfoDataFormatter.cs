using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AttendanceInfoDataFormatter : IMessagePackFormatter<AttendanceInfoData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AttendanceInfoData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DescKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.IconStatus, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
		writer.Write(value.SendMail);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MenuNames, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NextStepPopupTitle, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NextStepPopupDesc, options);
		resolver.GetFormatterWithVerify<QuestOpenConditionType>().Serialize(ref writer, value.ConditionType1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionValue1, options);
	}

	public AttendanceInfoData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AttendanceInfoData attendanceInfoData = new AttendanceInfoData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				attendanceInfoData.Index = reader.ReadInt32();
				break;
			case 1:
				attendanceInfoData.TitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				attendanceInfoData.DescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				attendanceInfoData.IconStatus = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				attendanceInfoData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				attendanceInfoData.SendMail = reader.ReadBoolean();
				break;
			case 6:
				attendanceInfoData.ShowIndex = reader.ReadInt32();
				break;
			case 7:
				attendanceInfoData.MenuNames = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 8:
				attendanceInfoData.NextStepPopupTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				attendanceInfoData.NextStepPopupDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				attendanceInfoData.ConditionType1 = resolver.GetFormatterWithVerify<QuestOpenConditionType>().Deserialize(ref reader, options);
				break;
			case 11:
				attendanceInfoData.ConditionValue1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return attendanceInfoData;
	}
}
