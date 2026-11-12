using NShared.NWorldMap;

namespace MessagePack.Formatters.NShared.NWorldMap;

public sealed class EventDataFormatter : IMessagePackFormatter<EventData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		writer.Write(value.Index);
		writer.Write(value.ChapterIndex);
		resolver.GetFormatterWithVerify<EventType>().Serialize(ref writer, value.EventType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DungeonIndex, options);
		writer.Write(value.CompletedDungeonOnly);
		writer.Write(value.CurrentChapterOnly);
		writer.Write(value.ReqChapterIndex);
		writer.Write(value.ReqDungeonIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.EventValue, options);
		writer.Write(value.OccurCount);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.OccurPercentage, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.OccurHour, options);
		writer.Write(value.Duration);
		writer.Write(value.Cooltime);
	}

	public EventData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventData eventData = new EventData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventData.Index = reader.ReadInt32();
				break;
			case 1:
				eventData.ChapterIndex = reader.ReadInt32();
				break;
			case 2:
				eventData.EventType = resolver.GetFormatterWithVerify<EventType>().Deserialize(ref reader, options);
				break;
			case 3:
				eventData.DungeonIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 4:
				eventData.CompletedDungeonOnly = reader.ReadBoolean();
				break;
			case 5:
				eventData.CurrentChapterOnly = reader.ReadBoolean();
				break;
			case 6:
				eventData.ReqChapterIndex = reader.ReadInt32();
				break;
			case 7:
				eventData.ReqDungeonIndex = reader.ReadInt32();
				break;
			case 8:
				eventData.EventValue = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 9:
				eventData.OccurCount = reader.ReadInt32();
				break;
			case 10:
				eventData.OccurPercentage = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 11:
				eventData.OccurHour = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				eventData.Duration = reader.ReadInt32();
				break;
			case 13:
				eventData.Cooltime = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventData;
	}
}
