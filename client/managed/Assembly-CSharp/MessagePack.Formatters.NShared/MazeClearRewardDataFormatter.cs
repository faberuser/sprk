using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MazeClearRewardDataFormatter : IMessagePackFormatter<MazeClearRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MazeClearRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		writer.Write(value.ClearFloor);
		writer.Write(value.RewardIndex);
	}

	public MazeClearRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		MazeClearRewardData mazeClearRewardData = new MazeClearRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				mazeClearRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				mazeClearRewardData.ClearFloor = reader.ReadInt32();
				break;
			case 2:
				mazeClearRewardData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return mazeClearRewardData;
	}
}
