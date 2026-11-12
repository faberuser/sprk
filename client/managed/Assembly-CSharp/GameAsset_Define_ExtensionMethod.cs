using NGameAsset;
using NVespa;
using UnityEngine;

public static class GameAsset_Define_ExtensionMethod
{
	public static Color GetMotionColor(this DefineCreature.EMotion This)
	{
		return This switch
		{
			DefineCreature.EMotion.Idle => Color.yellow, 
			DefineCreature.EMotion.BattleIdle => Color.yellow, 
			DefineCreature.EMotion.Run => Colors.orange, 
			DefineCreature.EMotion.Die => Color.red, 
			DefineCreature.EMotion.Damage => Vespa_Unity_Color_Extension_Method.Lerp(Color.red, Color.white, 0.5f), 
			DefineCreature.EMotion.Stun => Color.blue, 
			DefineCreature.EMotion.Down => Vespa_Unity_Color_Extension_Method.Lerp(Color.blue, Color.white, 0.5f), 
			DefineCreature.EMotion.Getup => Vespa_Unity_Color_Extension_Method.Lerp(Color.blue, Color.white, 0.5f), 
			DefineCreature.EMotion.Looting => Color.green, 
			DefineCreature.EMotion.NamedAction => Vespa_Unity_Color_Extension_Method.Lerp(Color.red, Color.white, 0.5f), 
			DefineCreature.EMotion.EmotiMotion => Color.magenta, 
			DefineCreature.EMotion.HouseMotion => Color.cyan, 
			_ => Color.white, 
		};
	}
}
