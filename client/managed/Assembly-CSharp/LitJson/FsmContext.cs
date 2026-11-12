namespace LitJson;

internal class FsmContext
{
	public bool Return;

	public int NextState;

	public LitJson.Lexer L;

	public int StateStack;
}
