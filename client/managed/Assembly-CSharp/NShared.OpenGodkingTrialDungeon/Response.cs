namespace NShared.OpenGodkingTrialDungeon;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GodkingTrialDungeonInfo GodkingTrialDungeonInfo { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
