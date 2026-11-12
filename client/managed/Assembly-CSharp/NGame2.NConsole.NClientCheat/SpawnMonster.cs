namespace NGame2.NConsole.NClientCheat;

public class SpawnMonster : ICheatCommand
{
	string ICheatCommand.Help()
	{
		return "spawnmonster [index] [star] [level] [tier]";
	}

	void ICheatCommand.Process(string[] tokens, out string output)
	{
		output = string.Empty;
	}
}
