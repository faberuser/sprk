using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignNodeDataFormatter : IMessagePackFormatter<CampaignNodeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignNodeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(38);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<NodeFunctionType>().Serialize(ref writer, value.Function, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.SwitchCondition, options);
		resolver.GetFormatterWithVerify<HashString>().Serialize(ref writer, value.NodePrefab, options);
		resolver.GetFormatterWithVerify<HashString>().Serialize(ref writer, value.FunctionButton, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Code, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Value, options);
		writer.Write(value.UiPositionX);
		writer.Write(value.UiPositionY);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.NextNodeIndex, options);
		writer.Write(value.Oneway);
		writer.Write(value.Opened);
		writer.Write(value.Hidden);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ResetNodeIndex, options);
		writer.Write(value.PortalPositionX);
		writer.Write(value.PortalPositionY);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortalNameCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortalImageCode, options);
		writer.Write(value.RecommendHeroLevel);
		writer.Write(value.VisitRewardIndex);
		writer.Write(value.ReqDungeonIndex);
		writer.Write(value.ReqTutorialIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ReqCondition, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.AddDirectionNode, options);
		resolver.GetFormatterWithVerify<CampaignNodeData.ENodeOpenStyle>().Serialize(ref writer, value.NodeOpenStyle, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.NodeOpenValues, options);
		writer.Write(value.OpenBackgroundDownloading);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryNPCCode, options);
		resolver.GetFormatterWithVerify<CampaignNodeData.EMenuOpenCondition>().Serialize(ref writer, value.MenuOpenCondition, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MenuOpenConditionArgs, options);
		resolver.GetFormatterWithVerify<StaminaType>().Serialize(ref writer, value.EntryMenuStamina, options);
		writer.Write(value.PortalListScore);
		resolver.GetFormatterWithVerify<ShopCostType>().Serialize(ref writer, value.EntryMenuAsset, options);
		resolver.GetFormatterWithVerify<ContentsStatusType>().Serialize(ref writer, value.ContentsType, options);
		writer.Write(value.ControlNPC);
		resolver.GetFormatterWithVerify<GuildConstructionResourceTopmostMenuType>().Serialize(ref writer, value.GuildResourceMenuType, options);
	}

	public CampaignNodeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignNodeData campaignNodeData = new CampaignNodeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				campaignNodeData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				campaignNodeData.DungeonIndex = reader.ReadInt32();
				break;
			case 3:
				campaignNodeData.Function = resolver.GetFormatterWithVerify<NodeFunctionType>().Deserialize(ref reader, options);
				break;
			case 4:
				campaignNodeData.SwitchCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				campaignNodeData.NodePrefab = resolver.GetFormatterWithVerify<HashString>().Deserialize(ref reader, options);
				break;
			case 6:
				campaignNodeData.FunctionButton = resolver.GetFormatterWithVerify<HashString>().Deserialize(ref reader, options);
				break;
			case 7:
				campaignNodeData.Code = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 8:
				campaignNodeData.Value = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				campaignNodeData.UiPositionX = reader.ReadInt32();
				break;
			case 10:
				campaignNodeData.UiPositionY = reader.ReadInt32();
				break;
			case 11:
				campaignNodeData.NextNodeIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				campaignNodeData.Oneway = reader.ReadBoolean();
				break;
			case 13:
				campaignNodeData.Opened = reader.ReadBoolean();
				break;
			case 14:
				campaignNodeData.Hidden = reader.ReadBoolean();
				break;
			case 15:
				campaignNodeData.ResetNodeIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 16:
				campaignNodeData.PortalPositionX = reader.ReadInt32();
				break;
			case 17:
				campaignNodeData.PortalPositionY = reader.ReadInt32();
				break;
			case 18:
				campaignNodeData.PortalNameCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				campaignNodeData.PortalImageCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 20:
				campaignNodeData.RecommendHeroLevel = reader.ReadInt32();
				break;
			case 21:
				campaignNodeData.VisitRewardIndex = reader.ReadInt32();
				break;
			case 22:
				campaignNodeData.ReqDungeonIndex = reader.ReadInt32();
				break;
			case 23:
				campaignNodeData.ReqTutorialIndex = reader.ReadInt32();
				break;
			case 24:
				campaignNodeData.ReqCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 25:
				campaignNodeData.AddDirectionNode = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 26:
				campaignNodeData.NodeOpenStyle = resolver.GetFormatterWithVerify<CampaignNodeData.ENodeOpenStyle>().Deserialize(ref reader, options);
				break;
			case 27:
				campaignNodeData.NodeOpenValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 28:
				campaignNodeData.OpenBackgroundDownloading = reader.ReadBoolean();
				break;
			case 29:
				campaignNodeData.EntryNPCCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 30:
				campaignNodeData.MenuOpenCondition = resolver.GetFormatterWithVerify<CampaignNodeData.EMenuOpenCondition>().Deserialize(ref reader, options);
				break;
			case 31:
				campaignNodeData.MenuOpenConditionArgs = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 32:
				campaignNodeData.EntryMenuStamina = resolver.GetFormatterWithVerify<StaminaType>().Deserialize(ref reader, options);
				break;
			case 33:
				campaignNodeData.PortalListScore = reader.ReadInt32();
				break;
			case 34:
				campaignNodeData.EntryMenuAsset = resolver.GetFormatterWithVerify<ShopCostType>().Deserialize(ref reader, options);
				break;
			case 35:
				campaignNodeData.ContentsType = resolver.GetFormatterWithVerify<ContentsStatusType>().Deserialize(ref reader, options);
				break;
			case 36:
				campaignNodeData.ControlNPC = reader.ReadBoolean();
				break;
			case 37:
				campaignNodeData.GuildResourceMenuType = resolver.GetFormatterWithVerify<GuildConstructionResourceTopmostMenuType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return campaignNodeData;
	}
}
