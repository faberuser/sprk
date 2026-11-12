using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GameOptionDataFormatter : IMessagePackFormatter<GameOptionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GameOptionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(25);
		writer.Write(value.Index);
		writer.Write(value.ParentNum);
		writer.Write(value.ChildRoot);
		writer.Write(value.SaveValue);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OptionName, options);
		resolver.GetFormatterWithVerify<GameOptionTab>().Serialize(ref writer, value.GameOptionTab, options);
		resolver.GetFormatterWithVerify<GameOptionComponent>().Serialize(ref writer, value.GameOptionComponent, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OptionDataType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ClassName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.FieldName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PostProcessFunction, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.UIdefaultValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ValueLocalKey1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Value1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DisableGroup1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ValueLocalKey2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Value2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DisableGroup2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ValueLocalKey3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Value3, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DisableGroup3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ValueLocalKey4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Value4, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DisableGroup4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ToolTipDesc, options);
	}

	public GameOptionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GameOptionData gameOptionData = new GameOptionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gameOptionData.Index = reader.ReadInt32();
				break;
			case 1:
				gameOptionData.ParentNum = reader.ReadInt32();
				break;
			case 2:
				gameOptionData.ChildRoot = reader.ReadBoolean();
				break;
			case 3:
				gameOptionData.SaveValue = reader.ReadBoolean();
				break;
			case 4:
				gameOptionData.OptionName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				gameOptionData.GameOptionTab = resolver.GetFormatterWithVerify<GameOptionTab>().Deserialize(ref reader, options);
				break;
			case 6:
				gameOptionData.GameOptionComponent = resolver.GetFormatterWithVerify<GameOptionComponent>().Deserialize(ref reader, options);
				break;
			case 7:
				gameOptionData.OptionDataType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				gameOptionData.ClassName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				gameOptionData.FieldName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				gameOptionData.PostProcessFunction = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				gameOptionData.UIdefaultValue = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				gameOptionData.ValueLocalKey1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				gameOptionData.Value1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				gameOptionData.DisableGroup1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 15:
				gameOptionData.ValueLocalKey2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				gameOptionData.Value2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				gameOptionData.DisableGroup2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 18:
				gameOptionData.ValueLocalKey3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				gameOptionData.Value3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 20:
				gameOptionData.DisableGroup3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 21:
				gameOptionData.ValueLocalKey4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				gameOptionData.Value4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 23:
				gameOptionData.DisableGroup4 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 24:
				gameOptionData.ToolTipDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gameOptionData;
	}
}
