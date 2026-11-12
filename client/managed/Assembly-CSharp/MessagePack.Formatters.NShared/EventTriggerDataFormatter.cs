using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventTriggerDataFormatter : IMessagePackFormatter<EventTriggerData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventTriggerData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EventTriggerCategory>().Serialize(ref writer, value.Category, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EventType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EventCondition, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.CurrentCondition, options);
		resolver.GetFormatterWithVerify<EventTriggerCommand>().Serialize(ref writer, value.Command, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Argument, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NpcCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EventTitle, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EventDescription, options);
		writer.Write(value.ImmediatelyLoadComplate);
		writer.Write(value.ForceLeaveParty);
	}

	public EventTriggerData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventTriggerData eventTriggerData = new EventTriggerData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventTriggerData.Index = reader.ReadInt32();
				break;
			case 1:
				eventTriggerData.Category = resolver.GetFormatterWithVerify<EventTriggerCategory>().Deserialize(ref reader, options);
				break;
			case 2:
				eventTriggerData.EventType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				eventTriggerData.EventCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				eventTriggerData.CurrentCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				eventTriggerData.Command = resolver.GetFormatterWithVerify<EventTriggerCommand>().Deserialize(ref reader, options);
				break;
			case 6:
				eventTriggerData.Argument = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				eventTriggerData.NpcCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				eventTriggerData.EventTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				eventTriggerData.EventDescription = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				eventTriggerData.ImmediatelyLoadComplate = reader.ReadBoolean();
				break;
			case 11:
				eventTriggerData.ForceLeaveParty = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventTriggerData;
	}
}
