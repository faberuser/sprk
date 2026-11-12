using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StateActionDataFormatter : IMessagePackFormatter<StateActionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StateActionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StateType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReqSkillIndex, options);
		resolver.GetFormatterWithVerify<StateConditionType>().Serialize(ref writer, value.ConditionType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ConditionValue, options);
		resolver.GetFormatterWithVerify<StateActionType>().Serialize(ref writer, value.ActionType1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionValue1, options);
		resolver.GetFormatterWithVerify<StateActionType>().Serialize(ref writer, value.ActionType2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionValue2, options);
		resolver.GetFormatterWithVerify<StateActionType>().Serialize(ref writer, value.ActionType3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActionValue3, options);
	}

	public StateActionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		StateActionData stateActionData = new StateActionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				stateActionData.StateType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				stateActionData.ReqSkillIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 2:
				stateActionData.ConditionType = resolver.GetFormatterWithVerify<StateConditionType>().Deserialize(ref reader, options);
				break;
			case 3:
				stateActionData.ConditionValue = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 4:
				stateActionData.ActionType1 = resolver.GetFormatterWithVerify<StateActionType>().Deserialize(ref reader, options);
				break;
			case 5:
				stateActionData.ActionValue1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 6:
				stateActionData.ActionType2 = resolver.GetFormatterWithVerify<StateActionType>().Deserialize(ref reader, options);
				break;
			case 7:
				stateActionData.ActionValue2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 8:
				stateActionData.ActionType3 = resolver.GetFormatterWithVerify<StateActionType>().Deserialize(ref reader, options);
				break;
			case 9:
				stateActionData.ActionValue3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return stateActionData;
	}
}
