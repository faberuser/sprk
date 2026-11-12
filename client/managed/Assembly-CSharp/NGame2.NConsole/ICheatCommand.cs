namespace NGame2.NConsole;

public interface ICheatCommand
{
	string Help();

	void Process(string[] tokens, out string output);
}
