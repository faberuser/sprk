using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MissionVisualDataFormatter : IMessagePackFormatter<MissionVisualData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MissionVisualData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.MainCategoryIndex);
		writer.Write(value.SubCategoryIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.QuestType, options);
		writer.Write(value.QuestIndex);
		writer.Write(value.MissionGradeValue);
		writer.Write(value.MainStep);
		writer.Write(value.SubStep);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NodeIconPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NodeIconLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MilestonePopupNpcCode, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.DetailDisplayLocalize, options);
	}

	public MissionVisualData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MissionVisualData missionVisualData = new MissionVisualData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				missionVisualData.MainCategoryIndex = reader.ReadInt32();
				break;
			case 1:
				missionVisualData.SubCategoryIndex = reader.ReadInt32();
				break;
			case 2:
				missionVisualData.QuestType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				missionVisualData.QuestIndex = reader.ReadInt32();
				break;
			case 4:
				missionVisualData.MissionGradeValue = reader.ReadInt32();
				break;
			case 5:
				missionVisualData.MainStep = reader.ReadInt32();
				break;
			case 6:
				missionVisualData.SubStep = reader.ReadInt32();
				break;
			case 7:
				missionVisualData.NodeIconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				missionVisualData.NodeIconLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				missionVisualData.MilestonePopupNpcCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				missionVisualData.DetailDisplayLocalize = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return missionVisualData;
	}
}
