using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NPCFriendlyPointDataFormatter : IMessagePackFormatter<NPCFriendlyPointData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NPCFriendlyPointData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		writer.Write(value.HeroIndex);
		writer.Write(value.Step);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EffectCode, options);
		writer.Write(value.ReqFriendlyPoint);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardDesc, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RestrictionItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RestrictionLocalKey, options);
		writer.Write(value.LimitFriendlyPoint);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.UIColor, options);
		writer.Write(value.EndRewardRepeatCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StampName, options);
	}

	public NPCFriendlyPointData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NPCFriendlyPointData nPCFriendlyPointData = new NPCFriendlyPointData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				nPCFriendlyPointData.HeroIndex = reader.ReadInt32();
				break;
			case 1:
				nPCFriendlyPointData.Step = reader.ReadInt32();
				break;
			case 2:
				nPCFriendlyPointData.EffectCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				nPCFriendlyPointData.ReqFriendlyPoint = reader.ReadInt32();
				break;
			case 4:
				nPCFriendlyPointData.RewardIndex = reader.ReadInt32();
				break;
			case 5:
				nPCFriendlyPointData.RewardDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				nPCFriendlyPointData.RestrictionItemCode = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				nPCFriendlyPointData.RestrictionLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				nPCFriendlyPointData.LimitFriendlyPoint = reader.ReadBoolean();
				break;
			case 9:
				nPCFriendlyPointData.UIColor = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 10:
				nPCFriendlyPointData.EndRewardRepeatCount = reader.ReadInt32();
				break;
			case 11:
				nPCFriendlyPointData.StampName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return nPCFriendlyPointData;
	}
}
