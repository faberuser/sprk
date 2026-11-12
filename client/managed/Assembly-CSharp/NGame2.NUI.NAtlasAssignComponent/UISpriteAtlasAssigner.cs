using UnityEngine;

namespace NGame2.NUI.NAtlasAssignComponent;

public class UISpriteAtlasAssigner : AtlasAssignerBase
{
	public UISprite Target;

	public string AtlasName;

	public string SpriteName;

	protected override void SetTargetComponent(Component component)
	{
		UISprite uISprite = component as UISprite;
		if (!(uISprite == null))
		{
			Target = uISprite;
		}
	}

	protected override void SetPriorityInternal(Component component)
	{
		base.Priorty = 1;
	}

	protected override void SaveAtlas()
	{
		if (!(Target == null) && !(Target.atlas == null) && !IsIncludeAtlas(Target.atlas.name))
		{
			AtlasName = Target.atlas.name;
			SpriteName = Target.spriteName;
			Target.atlas = null;
			Target.spriteName = string.Empty;
		}
	}

	protected override void AssignAtlas()
	{
		if (!(Target == null) && !(Target.atlas != null))
		{
			Target.atlas = GetAtlas(AtlasName);
			Target.spriteName = GetSpriteName();
		}
	}

	private string GetSpriteName()
	{
		return SpriteName;
	}
}
