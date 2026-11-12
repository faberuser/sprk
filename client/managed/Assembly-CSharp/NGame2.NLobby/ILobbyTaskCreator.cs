using NVespa.NTask;

namespace NGame2.NLobby;

public interface ILobbyTaskCreator
{
	ITask Create(string[] values);
}
