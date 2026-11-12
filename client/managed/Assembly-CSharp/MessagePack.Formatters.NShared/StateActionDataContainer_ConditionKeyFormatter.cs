using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StateActionDataContainer_ConditionKeyFormatter : IMessagePackFormatter<StateActionDataContainer.ConditionKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StateActionDataContainer.ConditionKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.State, options);
		resolver.GetFormatterWithVerify<StateConditionType>().Serialize(ref writer, value.ConditionType, options);
	}

	public StateActionDataContainer.ConditionKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		string state = null;
		StateConditionType conditionType = StateConditionType.None;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				state = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				conditionType = resolver.GetFormatterWithVerify<StateConditionType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		StateActionDataContainer.ConditionKey result = new StateActionDataContainer.ConditionKey(state, conditionType);
		reader.Depth--;
		return result;
	}
}
