using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventDungeonGroupDataFormatter : IMessagePackFormatter<EventDungeonGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventDungeonGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.ChapterIndex);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<EDungeonGroupType>().Serialize(ref writer, value.GroupType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonDesc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortraitPath, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DungeonIndices, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BackGroundPath, options);
	}

	public EventDungeonGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventDungeonGroupData eventDungeonGroupData = new EventDungeonGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventDungeonGroupData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				eventDungeonGroupData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				eventDungeonGroupData.GroupType = resolver.GetFormatterWithVerify<EDungeonGroupType>().Deserialize(ref reader, options);
				break;
			case 3:
				eventDungeonGroupData.DungeonName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				eventDungeonGroupData.DungeonDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				eventDungeonGroupData.PortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				eventDungeonGroupData.DungeonIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 7:
				eventDungeonGroupData.BackGroundPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventDungeonGroupData;
	}
}
