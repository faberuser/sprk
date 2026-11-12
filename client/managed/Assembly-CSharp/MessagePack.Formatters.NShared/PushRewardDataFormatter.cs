using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PushRewardDataFormatter : IMessagePackFormatter<PushRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PushRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		writer.Write(value.DayOfTheWeek);
		writer.Write(value.OpenTime);
		writer.Write(value.EndTime);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TouchAction, options);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NameKey, options);
	}

	public PushRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PushRewardData pushRewardData = new PushRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				pushRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				pushRewardData.DayOfTheWeek = reader.ReadInt32();
				break;
			case 2:
				pushRewardData.OpenTime = reader.ReadInt32();
				break;
			case 3:
				pushRewardData.EndTime = reader.ReadInt32();
				break;
			case 4:
				pushRewardData.TouchAction = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				pushRewardData.RewardIndex = reader.ReadInt32();
				break;
			case 6:
				pushRewardData.NameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return pushRewardData;
	}
}
