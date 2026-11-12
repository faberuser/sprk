namespace NGame2.NScenarioQuest;

public interface IScenarioQuestRuntime
{
	string[] StateValue { get; set; }

	void Run(string[] stateValue);

	bool IsEnd();
}
