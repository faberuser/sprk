using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class TeamLevelBuffStarComponent : LinkFactoryComponent, InitOnCreate
{
	public static string PrefabPath = "Prefab2/Component/GameOption/TeamLevelBuffStarComponent";

	public static PrefabAuto<TeamLevelBuffStarComponent> PrefabAuto = new PrefabAuto<TeamLevelBuffStarComponent>(PrefabPath);

	public UISprite Star;

	public UIGame3DWidget Effect;

	public void Open(bool isInitEnable)
	{
		if (isInitEnable)
		{
			Enable(isPlayEffect: false);
		}
		else
		{
			UnEnable();
		}
	}

	public void Enable(bool isPlayEffect = true)
	{
		UIHelper.EnableGameObject(Star, enable: true);
		UIHelper.EnableGameObject(Effect, isPlayEffect);
	}

	public void UnEnable()
	{
		UIHelper.EnableGameObject(Star, enable: false);
		UIHelper.EnableGameObject(Effect, enable: false);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
