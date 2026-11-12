using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StaminaConsumePointDataFormatter : IMessagePackFormatter<StaminaConsumePointData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StaminaConsumePointData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.CurrencyType, options);
		writer.Write(value.DailyMaxValue);
		writer.Write(value.MaxValue);
		resolver.GetFormatterWithVerify<StaminaType[]>().Serialize(ref writer, value.StaminaType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.Multiple, options);
	}

	public StaminaConsumePointData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		StaminaConsumePointData staminaConsumePointData = new StaminaConsumePointData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				staminaConsumePointData.CurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 1:
				staminaConsumePointData.DailyMaxValue = reader.ReadInt32();
				break;
			case 2:
				staminaConsumePointData.MaxValue = reader.ReadInt32();
				break;
			case 3:
				staminaConsumePointData.StaminaType = resolver.GetFormatterWithVerify<StaminaType[]>().Deserialize(ref reader, options);
				break;
			case 4:
				staminaConsumePointData.Multiple = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return staminaConsumePointData;
	}
}
