namespace NVespa.NNGUIExtension.NAnimation;

public class BaseUpdateValue : IDrawInspector
{
	public virtual bool IsWaiting(float value)
	{
		return false;
	}

	public virtual bool CanUpdate(float value)
	{
		return false;
	}

	protected virtual void UpdateValue(float value)
	{
	}

	public virtual bool IsEnd()
	{
		return true;
	}

	public virtual void Reset()
	{
	}

	public virtual float GetRatio()
	{
		return 0f;
	}

	public virtual void Replay()
	{
	}

	public virtual void DrawInspector()
	{
	}

	public virtual void DrawInspectorParts()
	{
	}
}
