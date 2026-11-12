public sealed class SprkIllustrationProgress
{
	private readonly object sync = new object();

	private string stage = "Preparing";

	private float fraction = -1f;

	public void Report(string text, float value)
	{
		lock (sync)
		{
			stage = text;
			fraction = value;
		}
	}

	public void Read(out string text, out float value)
	{
		lock (sync)
		{
			text = stage;
			value = fraction;
		}
	}
}
