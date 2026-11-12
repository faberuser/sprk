namespace NGame2;

public interface IMessage
{
	bool IsDummy { get; set; }

	void ReleaseSelf();
}
