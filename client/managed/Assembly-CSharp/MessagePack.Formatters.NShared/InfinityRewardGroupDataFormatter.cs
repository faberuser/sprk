using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class InfinityRewardGroupDataFormatter : IMessagePackFormatter<InfinityRewardGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, InfinityRewardGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RewardIndices, options);
	}

	public InfinityRewardGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		InfinityRewardGroupData infinityRewardGroupData = new InfinityRewardGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				infinityRewardGroupData.Index = reader.ReadInt32();
				break;
			case 1:
				infinityRewardGroupData.RewardIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return infinityRewardGroupData;
	}
}
