namespace NGame2.NUI.NManager;

public interface IWindowManager<T>
{
	T Manager { get; }

	void Init<T1>(T1 t);
}
