using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

[Serializable]
public class BaseAnimationPlayData : IValidation, IDrawInspector
{
	public bool IgnoreTimeScale;

	public bool IsReverse;

	public virtual bool IsValid()
	{
		return false;
	}

	public virtual List<IUpdater> GetUpdaterList(GameObject[] objs, CreateUpdaterStock stock)
	{
		return null;
	}

	public virtual void DrawInspector()
	{
	}

	public virtual void DrawInspectorParts()
	{
	}
}
