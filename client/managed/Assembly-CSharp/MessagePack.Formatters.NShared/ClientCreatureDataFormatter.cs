using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ClientCreatureDataFormatter : IMessagePackFormatter<ClientCreatureData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ClientCreatureData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(40);
		resolver.GetFormatterWithVerify<HashString>().Serialize(ref writer, value.Code, options);
		writer.Write(value.CostumeIndex);
		writer.Write(value.HeroViewZoomInViewSize);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.HeroViewZoomInOffset, options);
		writer.Write(value.HeroViewZoomOutViewSize);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.HeroViewZoomOutOffset, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.DetailViewZoomInOffset, options);
		writer.Write(value.DetailViewZoomInDistance);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.DetailViewZoomOutOffset, options);
		writer.Write(value.DetailViewZoomOutDistance);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.PurifyDarknessOffset, options);
		writer.Write(value.PurifyCameraScale);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.PurifyCameraOffset, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.PayShopOffset, options);
		writer.Write(value.PayShopScale);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.HeroInnOffset, options);
		writer.Write(value.HeroInnScale);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroInnMotion, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.EntryMenuOffset, options);
		writer.Write(value.EntryMenuScale);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.MatchPickBanOffset, options);
		writer.Write(value.MatchPickBanScale);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.FullViewZoomInOffset, options);
		writer.Write(value.FullViewZoomInDistance);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.FullViewZoomOutOffset, options);
		writer.Write(value.FullViewZoomOutDistance);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.ArchiveViewZoomInOffset, options);
		writer.Write(value.ArchiveViewZoomInDistance);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.ArchiveViewZoomOutOffset, options);
		writer.Write(value.ArchiveViewZoomOutDistance);
		writer.Write(value.ArchiveCameraAngleX);
		writer.Write(value.ArchiveCameraAngleY);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ArchivePreviewBg, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.StepUpGachaOffset, options);
		writer.Write(value.StepUpScale);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PayShopAnimationKey, options);
		writer.Write(value.LookRotationX);
		writer.Write(value.LookRotationZ);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.PetListOffset, options);
		writer.Write(value.PetListScale);
	}

	public ClientCreatureData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ClientCreatureData clientCreatureData = new ClientCreatureData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				clientCreatureData.Code = resolver.GetFormatterWithVerify<HashString>().Deserialize(ref reader, options);
				break;
			case 1:
				clientCreatureData.CostumeIndex = reader.ReadInt32();
				break;
			case 2:
				clientCreatureData.HeroViewZoomInViewSize = reader.ReadSingle();
				break;
			case 3:
				clientCreatureData.HeroViewZoomInOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 4:
				clientCreatureData.HeroViewZoomOutViewSize = reader.ReadSingle();
				break;
			case 5:
				clientCreatureData.HeroViewZoomOutOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 6:
				clientCreatureData.DetailViewZoomInOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 7:
				clientCreatureData.DetailViewZoomInDistance = reader.ReadSingle();
				break;
			case 8:
				clientCreatureData.DetailViewZoomOutOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 9:
				clientCreatureData.DetailViewZoomOutDistance = reader.ReadSingle();
				break;
			case 10:
				clientCreatureData.PurifyDarknessOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 11:
				clientCreatureData.PurifyCameraScale = reader.ReadSingle();
				break;
			case 12:
				clientCreatureData.PurifyCameraOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 13:
				clientCreatureData.PayShopOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 14:
				clientCreatureData.PayShopScale = reader.ReadSingle();
				break;
			case 15:
				clientCreatureData.HeroInnOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 16:
				clientCreatureData.HeroInnScale = reader.ReadSingle();
				break;
			case 17:
				clientCreatureData.HeroInnMotion = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				clientCreatureData.EntryMenuOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 19:
				clientCreatureData.EntryMenuScale = reader.ReadSingle();
				break;
			case 20:
				clientCreatureData.MatchPickBanOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 21:
				clientCreatureData.MatchPickBanScale = reader.ReadSingle();
				break;
			case 22:
				clientCreatureData.FullViewZoomInOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 23:
				clientCreatureData.FullViewZoomInDistance = reader.ReadSingle();
				break;
			case 24:
				clientCreatureData.FullViewZoomOutOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 25:
				clientCreatureData.FullViewZoomOutDistance = reader.ReadSingle();
				break;
			case 26:
				clientCreatureData.ArchiveViewZoomInOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 27:
				clientCreatureData.ArchiveViewZoomInDistance = reader.ReadSingle();
				break;
			case 28:
				clientCreatureData.ArchiveViewZoomOutOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 29:
				clientCreatureData.ArchiveViewZoomOutDistance = reader.ReadSingle();
				break;
			case 30:
				clientCreatureData.ArchiveCameraAngleX = reader.ReadSingle();
				break;
			case 31:
				clientCreatureData.ArchiveCameraAngleY = reader.ReadSingle();
				break;
			case 32:
				clientCreatureData.ArchivePreviewBg = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 33:
				clientCreatureData.StepUpGachaOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 34:
				clientCreatureData.StepUpScale = reader.ReadSingle();
				break;
			case 35:
				clientCreatureData.PayShopAnimationKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 36:
				clientCreatureData.LookRotationX = reader.ReadSingle();
				break;
			case 37:
				clientCreatureData.LookRotationZ = reader.ReadSingle();
				break;
			case 38:
				clientCreatureData.PetListOffset = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 39:
				clientCreatureData.PetListScale = reader.ReadSingle();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return clientCreatureData;
	}
}
