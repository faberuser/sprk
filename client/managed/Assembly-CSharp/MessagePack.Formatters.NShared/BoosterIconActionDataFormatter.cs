using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BoosterIconActionDataFormatter : IMessagePackFormatter<BoosterIconActionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BoosterIconActionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EventBonusType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MoveTaskType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MoveTaskValue, options);
	}

	public BoosterIconActionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		BoosterIconActionData boosterIconActionData = new BoosterIconActionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				boosterIconActionData.EventBonusType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				boosterIconActionData.MoveTaskType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				boosterIconActionData.MoveTaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return boosterIconActionData;
	}
}
