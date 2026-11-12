namespace NGame2.NBattleThread;

public interface IThreadReceiveQueue<T>
{
	void Sync();

	bool IsEmpty();

	T Dequeue();
}
