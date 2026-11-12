using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewbieRewardProductMissionDataFormatter : IMessagePackFormatter<NewbieRewardProductMissionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewbieRewardProductMissionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<NewbieRewardProvideType>().Serialize(ref writer, value.ProvideType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition, options);
		writer.Write(value.RewardIndex);
		writer.Write(value.ProductGroupIndex);
	}

	public NewbieRewardProductMissionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewbieRewardProductMissionData newbieRewardProductMissionData = new NewbieRewardProductMissionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newbieRewardProductMissionData.Index = reader.ReadInt32();
				break;
			case 1:
				newbieRewardProductMissionData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				newbieRewardProductMissionData.ProvideType = resolver.GetFormatterWithVerify<NewbieRewardProvideType>().Deserialize(ref reader, options);
				break;
			case 3:
				newbieRewardProductMissionData.Condition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				newbieRewardProductMissionData.RewardIndex = reader.ReadInt32();
				break;
			case 5:
				newbieRewardProductMissionData.ProductGroupIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newbieRewardProductMissionData;
	}
}
