using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventStepDataFormatter : IMessagePackFormatter<EventStepData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventStepData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.CurrencyType, options);
		resolver.GetFormatterWithVerify<EventStepType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.Step);
		writer.Write(value.TargetScore);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WorldTreeIdleName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WorldTreeGrowName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GetRewardName, options);
	}

	public EventStepData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventStepData eventStepData = new EventStepData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventStepData.Index = reader.ReadInt32();
				break;
			case 1:
				eventStepData.CurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 2:
				eventStepData.Type = resolver.GetFormatterWithVerify<EventStepType>().Deserialize(ref reader, options);
				break;
			case 3:
				eventStepData.Step = reader.ReadInt32();
				break;
			case 4:
				eventStepData.TargetScore = reader.ReadInt32();
				break;
			case 5:
				eventStepData.RewardIndex = reader.ReadInt32();
				break;
			case 6:
				eventStepData.WorldTreeIdleName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				eventStepData.WorldTreeGrowName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				eventStepData.GetRewardName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventStepData;
	}
}
