using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignMultiAvatarDataFormatter : IMessagePackFormatter<CampaignMultiAvatarData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignMultiAvatarData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.LineIndex);
		writer.Write(value.ChapterIndex);
		writer.Write(value.Opened);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.NextStartCondition, options);
		writer.Write(value.CostumeIndex);
		writer.Write(value.StartNode);
		writer.Write(value.PairCostumeIndex);
		writer.Write(value.PairStartNode);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PairStatus, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CenterPopupKey, options);
		resolver.GetFormatterWithVerify<PairEffectType>().Serialize(ref writer, value.PairEffectType, options);
	}

	public CampaignMultiAvatarData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignMultiAvatarData campaignMultiAvatarData = new CampaignMultiAvatarData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				campaignMultiAvatarData.LineIndex = reader.ReadInt32();
				break;
			case 1:
				campaignMultiAvatarData.ChapterIndex = reader.ReadInt32();
				break;
			case 2:
				campaignMultiAvatarData.Opened = reader.ReadBoolean();
				break;
			case 3:
				campaignMultiAvatarData.NextStartCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				campaignMultiAvatarData.CostumeIndex = reader.ReadInt32();
				break;
			case 5:
				campaignMultiAvatarData.StartNode = reader.ReadInt32();
				break;
			case 6:
				campaignMultiAvatarData.PairCostumeIndex = reader.ReadInt32();
				break;
			case 7:
				campaignMultiAvatarData.PairStartNode = reader.ReadInt32();
				break;
			case 8:
				campaignMultiAvatarData.PairStatus = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				campaignMultiAvatarData.CenterPopupKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				campaignMultiAvatarData.PairEffectType = resolver.GetFormatterWithVerify<PairEffectType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return campaignMultiAvatarData;
	}
}
