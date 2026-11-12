namespace NShared.ReinforceTeamLevelBuff;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/reinforce_team_level_buff";

	public int BuffTeamLevel { get; set; }

	public int OptionIndex { get; set; }

	public int BonusOptionIndex { get; set; }

	public int Reinforce { get; set; }

	public int ReinforcePrice { get; set; }

	public string ReinforcePriceType { get; set; }
}
