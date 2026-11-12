using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignNodeNPCDataFormatter : IMessagePackFormatter<CampaignNodeNPCData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignNodeNPCData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.AppearMotionValues, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.AliveMotionValues, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.DeadMotionValues, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.LoseMotionValues, options);
	}

	public CampaignNodeNPCData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignNodeNPCData campaignNodeNPCData = new CampaignNodeNPCData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				campaignNodeNPCData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				campaignNodeNPCData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				campaignNodeNPCData.AppearMotionValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 3:
				campaignNodeNPCData.AliveMotionValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				campaignNodeNPCData.DeadMotionValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				campaignNodeNPCData.LoseMotionValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return campaignNodeNPCData;
	}
}
