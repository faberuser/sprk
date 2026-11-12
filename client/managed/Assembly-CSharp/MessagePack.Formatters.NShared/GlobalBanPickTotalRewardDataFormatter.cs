using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GlobalBanPickTotalRewardDataFormatter : IMessagePackFormatter<GlobalBanPickTotalRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GlobalBanPickTotalRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ServerBuffCode, options);
	}

	public GlobalBanPickTotalRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GlobalBanPickTotalRewardData globalBanPickTotalRewardData = new GlobalBanPickTotalRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				globalBanPickTotalRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				globalBanPickTotalRewardData.ServerBuffCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return globalBanPickTotalRewardData;
	}
}
