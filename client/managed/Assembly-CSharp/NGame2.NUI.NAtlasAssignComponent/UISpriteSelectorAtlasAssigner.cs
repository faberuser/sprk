using System;
using System.Collections.Generic;
using NNguiExtension;
using UnityEngine;

namespace NGame2.NUI.NAtlasAssignComponent;

public class UISpriteSelectorAtlasAssigner : AtlasAssignerBase
{
	[Serializable]
	public class AtlasSaveData
	{
		public string AtlasName = string.Empty;

		public string SpriteName = string.Empty;

		public AtlasSaveData(string atlasName, string spriteName)
		{
			AtlasName = atlasName;
			SpriteName = spriteName;
		}

		public bool IsValid()
		{
			return AtlasName != string.Empty;
		}
	}

	public UISpriteSelector Target;

	public List<AtlasSaveData> SelectList = new List<AtlasSaveData>();

	protected override void SetTargetComponent(Component component)
	{
		UISpriteSelector uISpriteSelector = component as UISpriteSelector;
		if (!(uISpriteSelector == null))
		{
			Target = uISpriteSelector;
		}
	}

	protected override void SetPriorityInternal(Component component)
	{
		base.Priorty = 2;
	}

	protected override void SaveAtlas()
	{
		if (Target == null || __Extension_Method_ICollection__.COUNT(Target.SelectList) == 0 || __Extension_Method_ICollection__.COUNT(Target.SelectList) == __Extension_Method_ICollection__.COUNT(SelectList))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Target.SelectList); i++)
		{
			AtlasSaveData atlasSaveData = null;
			UISpriteSelector.SelectData selectData = Target.SelectList[i];
			UIAtlas atlas = selectData.Atlas;
			string atlasName = ((atlas == null) ? string.Empty : atlas.name);
			string spriteName = selectData.SpriteName;
			if (IsIncludeAtlas(atlasName))
			{
				atlasSaveData = new AtlasSaveData(string.Empty, string.Empty);
			}
			else
			{
				selectData.Atlas = null;
				atlasSaveData = new AtlasSaveData(atlasName, spriteName);
			}
			SelectList.Add(atlasSaveData);
		}
	}

	protected override void AssignAtlas()
	{
		if (Target == null || __Extension_Method_ICollection__.COUNT(Target.SelectList) != __Extension_Method_ICollection__.COUNT(SelectList))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SelectList); i++)
		{
			AtlasSaveData atlasSaveData = SelectList[i];
			if (atlasSaveData.IsValid())
			{
				Target.SelectList[i].Atlas = GetAtlas(atlasSaveData.AtlasName);
				Target.SelectList[i].SpriteName = atlasSaveData.SpriteName;
			}
		}
	}
}
