using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StepUpGachaRewardDataFormatter : IMessagePackFormatter<StepUpGachaRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StepUpGachaRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		writer.Write(value.Step);
		writer.Write(value.Round);
		writer.Write(value.NextStep);
		writer.Write(value.RewardIndex);
		writer.Write(value.Discount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Buff, options);
	}

	public StepUpGachaRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		StepUpGachaRewardData stepUpGachaRewardData = new StepUpGachaRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				stepUpGachaRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				stepUpGachaRewardData.Step = reader.ReadInt32();
				break;
			case 2:
				stepUpGachaRewardData.Round = reader.ReadInt32();
				break;
			case 3:
				stepUpGachaRewardData.NextStep = reader.ReadInt32();
				break;
			case 4:
				stepUpGachaRewardData.RewardIndex = reader.ReadInt32();
				break;
			case 5:
				stepUpGachaRewardData.Discount = reader.ReadInt32();
				break;
			case 6:
				stepUpGachaRewardData.Buff = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return stepUpGachaRewardData;
	}
}
