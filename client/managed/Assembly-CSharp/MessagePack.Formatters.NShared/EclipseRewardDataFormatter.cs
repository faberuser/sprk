using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EclipseRewardDataFormatter : IMessagePackFormatter<EclipseRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EclipseRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.Wave);
		writer.Write(value.NormalReward);
		writer.Write(value.BestTeamReward);
	}

	public EclipseRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EclipseRewardData eclipseRewardData = new EclipseRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eclipseRewardData.Wave = reader.ReadInt32();
				break;
			case 1:
				eclipseRewardData.NormalReward = reader.ReadInt32();
				break;
			case 2:
				eclipseRewardData.BestTeamReward = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eclipseRewardData;
	}
}
