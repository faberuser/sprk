using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NUI.NWindow;
using NShared;
using NShared.BeginContent;

public static class SprkSoloRetry
{
	public static bool Allow(IDataProvider provider, BattleRetryConditionType condition)
	{
		if (provider == null || condition != BattleRetryConditionType.Lose || (!(provider is CampaignContext) && !(provider is PunishmentRaidContext)) || provider.IsOnline())
		{
			return false;
		}
		EnterDungeonData enterDungeon = provider.GetEnterDungeon();
		if (enterDungeon == null || enterDungeon.OriginalDungeonData == null)
		{
			return false;
		}
		switch ((int)provider.GetBattleType())
		{
		case 1:
		case 2:
		case 10:
		case 12:
		case 21:
		case 23:
		case 24:
		case 25:
		case 29:
		case 30:
		case 33:
		case 40:
		case 41:
		case 42:
		case 45:
		case 46:
		case 47:
		case 48:
			return true;
		default:
			return false;
		}
	}

	public static void Retry(BattleContext context, BattlePartySetting.ExtraOptionData option)
	{
		if (context.DataProvider is PunishmentRaidContext)
		{
			context.RepeatContent(option);
		}
		else
		{
			context.RepeatCampaign(option);
		}
	}

	public static void PreserveShards(NShared.BeginContent.Request request, PunishmentRaidContext source)
	{
		if (source.PunishmentDungeonType == EPunishmentDungeonType.Boss)
		{
			request.FlaskItemIndex = source.ExtraData.FlaskItemIndex;
			request.FlaskItemCount = source.ExtraData.FlaskItemCount;
		}
	}
}
