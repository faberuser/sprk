namespace NGame2.NBattleThread;

public interface IThreadSendQueue<T>
{
	void Enqueue(T value);

	void Flush();
}
