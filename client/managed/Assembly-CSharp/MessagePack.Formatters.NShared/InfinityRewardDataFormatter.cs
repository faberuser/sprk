using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class InfinityRewardDataFormatter : IMessagePackFormatter<InfinityRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, InfinityRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<RewardType>().Serialize(ref writer, value.RewardType, options);
		writer.Write(value.ItemIndex);
		writer.Write(value.IsRatio);
		writer.Write(value.InfinityIndex);
	}

	public InfinityRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		InfinityRewardData infinityRewardData = new InfinityRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				infinityRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				infinityRewardData.RewardType = resolver.GetFormatterWithVerify<RewardType>().Deserialize(ref reader, options);
				break;
			case 2:
				infinityRewardData.ItemIndex = reader.ReadInt32();
				break;
			case 3:
				infinityRewardData.IsRatio = reader.ReadBoolean();
				break;
			case 4:
				infinityRewardData.InfinityIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return infinityRewardData;
	}
}
