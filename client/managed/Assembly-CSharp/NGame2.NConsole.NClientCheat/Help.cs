using NShared;
using NVespa;

namespace NGame2.NConsole.NClientCheat;

public class Help : ICheatCommand
{
	string ICheatCommand.Help()
	{
		return "help [command]";
	}

	void ICheatCommand.Process(string[] tokens, out string output)
	{
		string valueString = ArgParser.GetValueString(tokens, 1);
		output = SingletonAuto<ConsoleInputManager>.instance.GetHelpMessage(valueString);
	}
}
