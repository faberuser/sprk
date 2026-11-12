using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignFieldDataFormatter : IMessagePackFormatter<CampaignFieldData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignFieldData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		writer.Write(value.BattleAreaGroupIndex1);
		writer.Write(value.AreaBottomM1);
		writer.Write(value.AreaTopM1);
		writer.Write(value.AreaLeftM1);
		writer.Write(value.AreaRightM1);
		writer.Write(value.CenterBiasM1);
		writer.Write(value.Angle1);
		writer.Write(value.BattleAreaGroupIndex2);
		writer.Write(value.AreaBottomM2);
		writer.Write(value.AreaTopM2);
		writer.Write(value.AreaLeftM2);
		writer.Write(value.AreaRightM2);
		writer.Write(value.CenterBiasM2);
		writer.Write(value.Angle2);
	}

	public CampaignFieldData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignFieldData campaignFieldData = new CampaignFieldData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				campaignFieldData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				campaignFieldData.BattleAreaGroupIndex1 = reader.ReadInt32();
				break;
			case 2:
				campaignFieldData.AreaBottomM1 = reader.ReadInt32();
				break;
			case 3:
				campaignFieldData.AreaTopM1 = reader.ReadInt32();
				break;
			case 4:
				campaignFieldData.AreaLeftM1 = reader.ReadInt32();
				break;
			case 5:
				campaignFieldData.AreaRightM1 = reader.ReadInt32();
				break;
			case 6:
				campaignFieldData.CenterBiasM1 = reader.ReadInt32();
				break;
			case 7:
				campaignFieldData.Angle1 = reader.ReadInt32();
				break;
			case 8:
				campaignFieldData.BattleAreaGroupIndex2 = reader.ReadInt32();
				break;
			case 9:
				campaignFieldData.AreaBottomM2 = reader.ReadInt32();
				break;
			case 10:
				campaignFieldData.AreaTopM2 = reader.ReadInt32();
				break;
			case 11:
				campaignFieldData.AreaLeftM2 = reader.ReadInt32();
				break;
			case 12:
				campaignFieldData.AreaRightM2 = reader.ReadInt32();
				break;
			case 13:
				campaignFieldData.CenterBiasM2 = reader.ReadInt32();
				break;
			case 14:
				campaignFieldData.Angle2 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return campaignFieldData;
	}
}
