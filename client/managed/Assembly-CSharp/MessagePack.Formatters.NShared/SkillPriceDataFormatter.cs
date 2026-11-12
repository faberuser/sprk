using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillPriceDataFormatter : IMessagePackFormatter<SkillPriceData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillPriceData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.SlotIndex);
		writer.Write(value.Level);
		writer.Write(value.ReqGold);
		writer.Write(value.ReqItemCount);
	}

	public SkillPriceData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		SkillPriceData skillPriceData = new SkillPriceData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				skillPriceData.SlotIndex = reader.ReadInt32();
				break;
			case 1:
				skillPriceData.Level = reader.ReadInt32();
				break;
			case 2:
				skillPriceData.ReqGold = reader.ReadInt32();
				break;
			case 3:
				skillPriceData.ReqItemCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return skillPriceData;
	}
}
