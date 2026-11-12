using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EntryMenuSettingDataFormatter : IMessagePackFormatter<EntryMenuSettingData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EntryMenuSettingData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(39);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MenuType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EventBgmCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AssetNguiCode, options);
		writer.Write(value.PopupInterval);
		writer.Write(value.CameraZoomScale);
		writer.Write(value.CameraOffsetX);
		writer.Write(value.CameraOffsetY);
		writer.Write(value.CameraOffsetZ);
		writer.Write(value.CameraDirectionX);
		writer.Write(value.CameraDirectionY);
		writer.Write(value.CameraDirectionZ);
		writer.Write(value.AvatarDirAngle);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NpcCode, options);
		writer.Write(value.NpcStar);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NpcPrefix, options);
		writer.Write(value.NpcPositionX);
		writer.Write(value.NpcPositionY);
		writer.Write(value.NpcPositionZ);
		writer.Write(value.NpcDirectionX);
		writer.Write(value.NpcDirectionY);
		writer.Write(value.NpcDirectionZ);
		writer.Write(value.NpcScale);
		writer.Write(value.NpcSlideOffset);
		writer.Write(value.NpcSlideTimeLength);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NpcCode2, options);
		writer.Write(value.NpcStar2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.NpcPrefix2, options);
		writer.Write(value.NpcPosition2X);
		writer.Write(value.NpcPosition2Y);
		writer.Write(value.NpcPosition2Z);
		writer.Write(value.NpcDirection2X);
		writer.Write(value.NpcDirection2Y);
		writer.Write(value.NpcDirection2Z);
		writer.Write(value.NpcScale2);
		writer.Write(value.NpcSlideOffset2);
		writer.Write(value.NpcSlideTimeLength2);
		writer.Write(value.UseNodeTree);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CinemaWindowName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LobbyWindowName, options);
	}

	public EntryMenuSettingData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EntryMenuSettingData entryMenuSettingData = new EntryMenuSettingData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				entryMenuSettingData.MenuType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				entryMenuSettingData.EventBgmCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				entryMenuSettingData.AssetNguiCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				entryMenuSettingData.PopupInterval = reader.ReadSingle();
				break;
			case 4:
				entryMenuSettingData.CameraZoomScale = reader.ReadSingle();
				break;
			case 5:
				entryMenuSettingData.CameraOffsetX = reader.ReadSingle();
				break;
			case 6:
				entryMenuSettingData.CameraOffsetY = reader.ReadSingle();
				break;
			case 7:
				entryMenuSettingData.CameraOffsetZ = reader.ReadSingle();
				break;
			case 8:
				entryMenuSettingData.CameraDirectionX = reader.ReadSingle();
				break;
			case 9:
				entryMenuSettingData.CameraDirectionY = reader.ReadSingle();
				break;
			case 10:
				entryMenuSettingData.CameraDirectionZ = reader.ReadSingle();
				break;
			case 11:
				entryMenuSettingData.AvatarDirAngle = reader.ReadSingle();
				break;
			case 12:
				entryMenuSettingData.NpcCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				entryMenuSettingData.NpcStar = reader.ReadInt32();
				break;
			case 14:
				entryMenuSettingData.NpcPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				entryMenuSettingData.NpcPositionX = reader.ReadSingle();
				break;
			case 16:
				entryMenuSettingData.NpcPositionY = reader.ReadSingle();
				break;
			case 17:
				entryMenuSettingData.NpcPositionZ = reader.ReadSingle();
				break;
			case 18:
				entryMenuSettingData.NpcDirectionX = reader.ReadSingle();
				break;
			case 19:
				entryMenuSettingData.NpcDirectionY = reader.ReadSingle();
				break;
			case 20:
				entryMenuSettingData.NpcDirectionZ = reader.ReadSingle();
				break;
			case 21:
				entryMenuSettingData.NpcScale = reader.ReadSingle();
				break;
			case 22:
				entryMenuSettingData.NpcSlideOffset = reader.ReadInt32();
				break;
			case 23:
				entryMenuSettingData.NpcSlideTimeLength = reader.ReadSingle();
				break;
			case 24:
				entryMenuSettingData.NpcCode2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 25:
				entryMenuSettingData.NpcStar2 = reader.ReadInt32();
				break;
			case 26:
				entryMenuSettingData.NpcPrefix2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 27:
				entryMenuSettingData.NpcPosition2X = reader.ReadSingle();
				break;
			case 28:
				entryMenuSettingData.NpcPosition2Y = reader.ReadSingle();
				break;
			case 29:
				entryMenuSettingData.NpcPosition2Z = reader.ReadSingle();
				break;
			case 30:
				entryMenuSettingData.NpcDirection2X = reader.ReadSingle();
				break;
			case 31:
				entryMenuSettingData.NpcDirection2Y = reader.ReadSingle();
				break;
			case 32:
				entryMenuSettingData.NpcDirection2Z = reader.ReadSingle();
				break;
			case 33:
				entryMenuSettingData.NpcScale2 = reader.ReadSingle();
				break;
			case 34:
				entryMenuSettingData.NpcSlideOffset2 = reader.ReadInt32();
				break;
			case 35:
				entryMenuSettingData.NpcSlideTimeLength2 = reader.ReadSingle();
				break;
			case 36:
				entryMenuSettingData.UseNodeTree = reader.ReadBoolean();
				break;
			case 37:
				entryMenuSettingData.CinemaWindowName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 38:
				entryMenuSettingData.LobbyWindowName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return entryMenuSettingData;
	}
}
