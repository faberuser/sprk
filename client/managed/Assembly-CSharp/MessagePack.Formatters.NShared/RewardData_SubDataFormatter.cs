using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RewardData_SubDataFormatter : IMessagePackFormatter<RewardData.SubData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RewardData.SubData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.No);
		resolver.GetFormatterWithVerify<HashString>().Serialize(ref writer, value.Code, options);
		writer.Write(value.Rate);
		writer.Write(value.CountMin);
		writer.WriteNil();
		writer.Write(value.CountMax);
		writer.Write(value.StarMin);
		writer.Write(value.StarMax);
		writer.Write(value.CustomOptionIndex);
	}

	public RewardData.SubData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RewardData.SubData subData = new RewardData.SubData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				subData.No = reader.ReadInt32();
				break;
			case 1:
				subData.Code = resolver.GetFormatterWithVerify<HashString>().Deserialize(ref reader, options);
				break;
			case 2:
				subData.Rate = reader.ReadInt32();
				break;
			case 3:
				subData.CountMin = reader.ReadInt32();
				break;
			case 5:
				subData.CountMax = reader.ReadInt32();
				break;
			case 6:
				subData.StarMin = reader.ReadInt32();
				break;
			case 7:
				subData.StarMax = reader.ReadInt32();
				break;
			case 8:
				subData.CustomOptionIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return subData;
	}
}
