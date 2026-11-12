using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NConsole.NClientCheat;

public class MoveChapter : ICheatCommand
{
	string ICheatCommand.Help()
	{
		return "movechapter [chapter_index] [dungeon_index=1]";
	}

	void ICheatCommand.Process(string[] tokens, out string output)
	{
		output = string.Empty;
		int valueInt = ArgParser.GetValueInt(tokens, 1);
		if (valueInt != 0)
		{
			int num = ArgParser.GetValueInt(tokens, 2);
			if (num == 0)
			{
				num = 1;
			}
			SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(valueInt, num);
		}
	}
}
