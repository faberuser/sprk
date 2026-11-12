using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OptionSellDataFormatter : IMessagePackFormatter<OptionSellData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OptionSellData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		resolver.GetFormatterWithVerify<OptionSellCategory>().Serialize(ref writer, value.Category, options);
		writer.Write(value.Value);
		writer.Write(value.BackGroundIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GradeStringKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceKey, options);
		resolver.GetFormatterWithVerify<OptionSellSelectType>().Serialize(ref writer, value.SelectType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.GradeDependency, options);
	}

	public OptionSellData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		OptionSellData optionSellData = new OptionSellData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				optionSellData.Category = resolver.GetFormatterWithVerify<OptionSellCategory>().Deserialize(ref reader, options);
				break;
			case 1:
				optionSellData.Value = reader.ReadInt32();
				break;
			case 2:
				optionSellData.BackGroundIndex = reader.ReadInt32();
				break;
			case 3:
				optionSellData.GradeStringKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				optionSellData.ReplaceKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				optionSellData.SelectType = resolver.GetFormatterWithVerify<OptionSellSelectType>().Deserialize(ref reader, options);
				break;
			case 6:
				optionSellData.GradeDependency = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return optionSellData;
	}
}
