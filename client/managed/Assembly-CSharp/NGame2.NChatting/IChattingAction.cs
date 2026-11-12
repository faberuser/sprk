using NShared;

namespace NGame2.NChatting;

public interface IChattingAction
{
	string GetName();

	string ConvertChat(ChattingActionData data);

	void Run(ChattingActionData data);
}
