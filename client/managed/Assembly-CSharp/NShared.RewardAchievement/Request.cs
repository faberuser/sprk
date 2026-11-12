namespace NShared.RewardAchievement;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "achievement/reward_achievement";

	public int[] AchievementIndices { get; set; }

	public int[] Steps { get; set; }
}
