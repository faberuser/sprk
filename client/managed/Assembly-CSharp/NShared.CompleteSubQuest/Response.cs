namespace NShared.CompleteSubQuest;

[Response]
public class Response : GenericResponse<ResultType>
{
	public SubQuestInfo[] SucceededSubQuestInfos { get; set; }

	public RewardResultInfo[] RewardResults { get; set; }
}
