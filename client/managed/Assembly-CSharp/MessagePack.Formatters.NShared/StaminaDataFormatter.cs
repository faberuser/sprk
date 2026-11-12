using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StaminaDataFormatter : IMessagePackFormatter<StaminaData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StaminaData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(17);
		resolver.GetFormatterWithVerify<StaminaType>().Serialize(ref writer, value.StaminaType, options);
		resolver.GetFormatterWithVerify<StaminaUpdateType>().Serialize(ref writer, value.UpdateType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AttributeName, options);
		resolver.GetFormatterWithVerify<ResetType>().Serialize(ref writer, value.ResetType, options);
		writer.Write(value.IsResetLimit);
		writer.Write(value.ResetLimitCount);
		resolver.GetFormatterWithVerify<StaminaValueType>().Serialize(ref writer, value.MaxCountType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MaxCountValue, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ResetCount, options);
		writer.Write(value.AllowOverflow);
		resolver.GetFormatterWithVerify<StaminaValueType>().Serialize(ref writer, value.RechargeCostType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RechargeGoldValue, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RechargeGemValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PopupStringKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AutoRepeatStopMsgKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroBonusTooltipDesc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StaminaTooltipDesc, options);
	}

	public StaminaData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		StaminaData staminaData = new StaminaData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				staminaData.StaminaType = resolver.GetFormatterWithVerify<StaminaType>().Deserialize(ref reader, options);
				break;
			case 1:
				staminaData.UpdateType = resolver.GetFormatterWithVerify<StaminaUpdateType>().Deserialize(ref reader, options);
				break;
			case 2:
				staminaData.AttributeName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				staminaData.ResetType = resolver.GetFormatterWithVerify<ResetType>().Deserialize(ref reader, options);
				break;
			case 4:
				staminaData.IsResetLimit = reader.ReadBoolean();
				break;
			case 5:
				staminaData.ResetLimitCount = reader.ReadInt32();
				break;
			case 6:
				staminaData.MaxCountType = resolver.GetFormatterWithVerify<StaminaValueType>().Deserialize(ref reader, options);
				break;
			case 7:
				staminaData.MaxCountValue = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				staminaData.ResetCount = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 9:
				staminaData.AllowOverflow = reader.ReadBoolean();
				break;
			case 10:
				staminaData.RechargeCostType = resolver.GetFormatterWithVerify<StaminaValueType>().Deserialize(ref reader, options);
				break;
			case 11:
				staminaData.RechargeGoldValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				staminaData.RechargeGemValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 13:
				staminaData.PopupStringKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				staminaData.AutoRepeatStopMsgKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				staminaData.HeroBonusTooltipDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				staminaData.StaminaTooltipDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return staminaData;
	}
}
