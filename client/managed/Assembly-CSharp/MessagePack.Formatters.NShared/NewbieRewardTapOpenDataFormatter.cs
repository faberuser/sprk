using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewbieRewardTapOpenDataFormatter : IMessagePackFormatter<NewbieRewardTapOpenData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewbieRewardTapOpenData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.TabIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TabName, options);
		resolver.GetFormatterWithVerify<NewbieRewardType>().Serialize(ref writer, value.TabType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition, options);
	}

	public NewbieRewardTapOpenData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewbieRewardTapOpenData newbieRewardTapOpenData = new NewbieRewardTapOpenData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newbieRewardTapOpenData.TabIndex = reader.ReadInt32();
				break;
			case 1:
				newbieRewardTapOpenData.TabName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				newbieRewardTapOpenData.TabType = resolver.GetFormatterWithVerify<NewbieRewardType>().Deserialize(ref reader, options);
				break;
			case 3:
				newbieRewardTapOpenData.Condition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newbieRewardTapOpenData;
	}
}
