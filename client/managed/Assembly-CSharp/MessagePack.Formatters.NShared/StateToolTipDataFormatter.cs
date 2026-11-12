using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StateToolTipDataFormatter : IMessagePackFormatter<StateToolTipData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StateToolTipData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StateType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StateName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StateDesc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
	}

	public StateToolTipData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		StateToolTipData stateToolTipData = new StateToolTipData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				stateToolTipData.Index = reader.ReadInt32();
				break;
			case 1:
				stateToolTipData.StateType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				stateToolTipData.StateName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				stateToolTipData.StateDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				stateToolTipData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return stateToolTipData;
	}
}
