using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipAwakenRatioDataFormatter : IMessagePackFormatter<EquipAwakenRatioData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipAwakenRatioData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.Star);
		writer.Write(value.AwakenPoint);
		writer.Write(value.SuccessRatio);
		writer.Write(value.FailBonusRatio);
	}

	public EquipAwakenRatioData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EquipAwakenRatioData equipAwakenRatioData = new EquipAwakenRatioData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipAwakenRatioData.Star = reader.ReadInt32();
				break;
			case 1:
				equipAwakenRatioData.AwakenPoint = reader.ReadInt32();
				break;
			case 2:
				equipAwakenRatioData.SuccessRatio = reader.ReadInt32();
				break;
			case 3:
				equipAwakenRatioData.FailBonusRatio = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipAwakenRatioData;
	}
}
