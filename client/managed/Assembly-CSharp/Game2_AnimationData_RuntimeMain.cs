using NGame2.NRender.NRenderFacade;

public static class Game2_AnimationData_RuntimeMain
{
	public static bool ISEND(this AnimationData.RuntimeData runtime)
	{
		return runtime?.IsEnd ?? true;
	}

	public static bool ISEND(this SubCreatureContoller.RuntimeData runtime)
	{
		return runtime?.IsEnd ?? true;
	}
}
