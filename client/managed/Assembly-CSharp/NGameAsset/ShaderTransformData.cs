namespace NGameAsset;

public class ShaderTransformData
{
	public float Angle;

	public bool DepthShift;

	public ShaderTransformData(float angle, bool depthShift)
	{
		Angle = angle;
		DepthShift = depthShift;
	}
}
