using NShared;

namespace NGame2.NUI.NManager;

public abstract class WebManager
{
	public interface IListener<T> where T : Response
	{
		void Success(T tResponse);

		void Fail(T tResponse);
	}
}
