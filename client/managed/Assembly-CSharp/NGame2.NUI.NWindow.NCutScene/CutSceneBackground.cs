using System;
using NShader;
using UnityEngine;

namespace NGame2.NUI.NWindow.NCutScene;

public class CutSceneBackground : FilteredBackground
{
	public const string PrefabPath = "Resources/Prefab2/Window/CutScene/CutSceneBackground";

	public float LetterBoxSize = 150f;

	public UIPanel LetterBoxPanel;

	public UITexture LetterBoxUp;

	public UITexture LetterBoxDown;

	public void Open(IFilter filter, Action onComplete)
	{
		Open(filter, skipAnim: false, onComplete);
	}

	public void Open(IFilter filter, bool skipAnim, Action onComplete)
	{
		UIHelper.EnableGameObject(LetterBoxPanel, enable: true);
		LetterBoxUp.transform.localPosition = Vector3.zero;
		LetterBoxDown.transform.localPosition = Vector3.zero;
		Open();
		OpenFilter(filter, skipAnim, onComplete);
	}

	protected override void UpdateFrame(float curveFrame)
	{
		base.UpdateFrame(curveFrame);
		int num = Mathf.RoundToInt(Mathf.Lerp(2f, LetterBoxSize, curveFrame));
		LetterBoxUp.transform.localPosition = Vespa_Unity_Vector3_Extension_Method.ReplaceY(LetterBoxUp.transform.localPosition, -num);
		LetterBoxDown.transform.localPosition = Vespa_Unity_Vector3_Extension_Method.ReplaceY(LetterBoxDown.transform.localPosition, num);
	}

	public string GetDebug()
	{
		bool flag = base.isActiveAndEnabled;
		if (BlurTexture == null && BlackTexture == null)
		{
			return $"[{typeof(CutSceneBackground).Name}]{flag}, all texture is null.";
		}
		string text = "";
		if (BlurTexture != null)
		{
			bool flag2 = BlurTexture.isActiveAndEnabled;
			bool flag3 = BlurTexture.mainTexture == null;
			int depth = BlurTexture.depth;
			text = $"[BlurTexture] Enabled:{flag2} MainTexture:{flag3} Depth:{depth}";
		}
		string text2 = "";
		if (BlackTexture != null)
		{
			bool flag4 = BlackTexture.isActiveAndEnabled;
			bool flag5 = BlackTexture.mainTexture == null;
			int depth2 = BlackTexture.depth;
			text2 = $"[BlackTexture] Enabled:{flag4} MainTexture:{flag5} Depth:{depth2}";
		}
		return $"[{typeof(CutSceneBackground).Name}]{flag},{text},{text2}";
	}
}
