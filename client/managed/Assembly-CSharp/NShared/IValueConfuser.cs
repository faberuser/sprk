namespace NShared;

public interface IValueConfuser<T>
{
	T Enc(T original);

	T Dec(T confused);
}
