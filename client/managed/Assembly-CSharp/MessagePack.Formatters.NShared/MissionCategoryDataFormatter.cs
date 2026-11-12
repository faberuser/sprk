using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MissionCategoryDataFormatter : IMessagePackFormatter<MissionCategoryData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MissionCategoryData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(25);
		writer.Write(value.MainCategoryIndex);
		writer.Write(value.SubCategoryIndex);
		resolver.GetFormatterWithVerify<EMissionType>().Serialize(ref writer, value.MissionType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MainCategoryLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubCategoryLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardQuestType, options);
		writer.Write(value.RewardQuestIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.UIPropName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubCategoryButtonPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.UIBackgroundPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenConditionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.OpenConditionValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenConditionLocalKey, options);
		writer.Write(value.MissionComponentValue);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ButtonPrefabName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubCategoryDescLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconLocalKey, options);
		writer.Write(value.IsReservePopup);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CtrAdid, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PopupTitleLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PopupDescLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PopupImageAssetPath, options);
		resolver.GetFormatterWithVerify<EMissionTopViewLayout>().Serialize(ref writer, value.TopViewLayoutType, options);
		resolver.GetFormatterWithVerify<EMissionTopViewButtonGroup>().Serialize(ref writer, value.TopViewButtonGroupType, options);
		resolver.GetFormatterWithVerify<EMissionTopViewRewardType>().Serialize(ref writer, value.TopViewRewardType, options);
	}

	public MissionCategoryData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MissionCategoryData missionCategoryData = new MissionCategoryData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				missionCategoryData.MainCategoryIndex = reader.ReadInt32();
				break;
			case 1:
				missionCategoryData.SubCategoryIndex = reader.ReadInt32();
				break;
			case 2:
				missionCategoryData.MissionType = resolver.GetFormatterWithVerify<EMissionType>().Deserialize(ref reader, options);
				break;
			case 3:
				missionCategoryData.MainCategoryLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				missionCategoryData.SubCategoryLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				missionCategoryData.RewardQuestType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				missionCategoryData.RewardQuestIndex = reader.ReadInt32();
				break;
			case 7:
				missionCategoryData.UIPropName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				missionCategoryData.SubCategoryButtonPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				missionCategoryData.UIBackgroundPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				missionCategoryData.OpenConditionType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				missionCategoryData.OpenConditionValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				missionCategoryData.OpenConditionLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				missionCategoryData.MissionComponentValue = reader.ReadInt32();
				break;
			case 14:
				missionCategoryData.ButtonPrefabName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				missionCategoryData.SubCategoryDescLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				missionCategoryData.IconLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				missionCategoryData.IsReservePopup = reader.ReadBoolean();
				break;
			case 18:
				missionCategoryData.CtrAdid = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				missionCategoryData.PopupTitleLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 20:
				missionCategoryData.PopupDescLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 21:
				missionCategoryData.PopupImageAssetPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				missionCategoryData.TopViewLayoutType = resolver.GetFormatterWithVerify<EMissionTopViewLayout>().Deserialize(ref reader, options);
				break;
			case 23:
				missionCategoryData.TopViewButtonGroupType = resolver.GetFormatterWithVerify<EMissionTopViewButtonGroup>().Deserialize(ref reader, options);
				break;
			case 24:
				missionCategoryData.TopViewRewardType = resolver.GetFormatterWithVerify<EMissionTopViewRewardType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return missionCategoryData;
	}
}
