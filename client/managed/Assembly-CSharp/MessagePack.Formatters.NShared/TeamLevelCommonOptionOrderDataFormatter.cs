using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TeamLevelCommonOptionOrderDataFormatter : IMessagePackFormatter<TeamLevelCommonOptionOrderData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TeamLevelCommonOptionOrderData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.OptionType, options);
		writer.Write(value.Priority);
	}

	public TeamLevelCommonOptionOrderData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		TeamLevelCommonOptionOrderData teamLevelCommonOptionOrderData = new TeamLevelCommonOptionOrderData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				teamLevelCommonOptionOrderData.OptionType = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 1:
				teamLevelCommonOptionOrderData.Priority = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return teamLevelCommonOptionOrderData;
	}
}
