using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PhaseStepRewardDataFormatter : IMessagePackFormatter<PhaseStepRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PhaseStepRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<PhaseStepRewardType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.Step);
		resolver.GetFormatterWithVerify<PhaseStepReqType>().Serialize(ref writer, value.ReqType, options);
		writer.Write(value.ReqValue);
		writer.Write(value.RewardIndex);
	}

	public PhaseStepRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PhaseStepRewardData phaseStepRewardData = new PhaseStepRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				phaseStepRewardData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				phaseStepRewardData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				phaseStepRewardData.Type = resolver.GetFormatterWithVerify<PhaseStepRewardType>().Deserialize(ref reader, options);
				break;
			case 3:
				phaseStepRewardData.Step = reader.ReadInt32();
				break;
			case 4:
				phaseStepRewardData.ReqType = resolver.GetFormatterWithVerify<PhaseStepReqType>().Deserialize(ref reader, options);
				break;
			case 5:
				phaseStepRewardData.ReqValue = reader.ReadInt32();
				break;
			case 6:
				phaseStepRewardData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return phaseStepRewardData;
	}
}
