using System.Collections;

namespace NGame2.NLobby;

public interface IActivity
{
	IEnumerator coStart();

	void OnRequestStop();
}
