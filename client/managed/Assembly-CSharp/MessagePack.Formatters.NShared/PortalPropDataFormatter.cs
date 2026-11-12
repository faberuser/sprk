using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PortalPropDataFormatter : IMessagePackFormatter<PortalPropData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PortalPropData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.PositionX);
		writer.Write(value.PositionY);
		resolver.GetFormatterWithVerify<PortalPropType>().Serialize(ref writer, value.PropType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PropName, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.PropAction, options);
		writer.Write(value.Scale);
		writer.Write(value.Angle);
		resolver.GetFormatterWithVerify<PortalPropConditionType>().Serialize(ref writer, value.ConditionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionArgs, options);
	}

	public PortalPropData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PortalPropData portalPropData = new PortalPropData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				portalPropData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				portalPropData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				portalPropData.PositionX = reader.ReadInt32();
				break;
			case 3:
				portalPropData.PositionY = reader.ReadInt32();
				break;
			case 4:
				portalPropData.PropType = resolver.GetFormatterWithVerify<PortalPropType>().Deserialize(ref reader, options);
				break;
			case 5:
				portalPropData.PropName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				portalPropData.PropAction = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				portalPropData.Scale = reader.ReadInt32();
				break;
			case 8:
				portalPropData.Angle = reader.ReadInt32();
				break;
			case 9:
				portalPropData.ConditionType = resolver.GetFormatterWithVerify<PortalPropConditionType>().Deserialize(ref reader, options);
				break;
			case 10:
				portalPropData.ConditionArgs = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return portalPropData;
	}
}
