using NGame2.NScene.NPreloader;
using UnityEngine;

public static class Game2_Scene_Preloader_ILoadingTime_ExtensionMethod
{
	public static string CalcRelativeTime(this BasePreloader.ILoadingTime This, float globalBegin, float globalEnd)
	{
		float num = Mathf.InverseLerp(globalBegin, globalEnd, This.BeginTime);
		float num2 = Mathf.InverseLerp(globalBegin, globalEnd, This.EndTime);
		return $"{This.EndTime - This.BeginTime:F3} ({num}~{num2})";
	}
}
