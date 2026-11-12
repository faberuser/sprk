namespace NShared.NMessage;

public interface IMessageNot
{
	long SenderId { get; }

	string Type { get; }

	string Content { get; }
}
