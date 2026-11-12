using NGame2.NCutScene;
using NVespa;

namespace NGame2.NBattleContext.NPredefine;

public class CutScene
{
	public static LocalStorageBool ForcePredefine = new LocalStorageBool(i_default: false, typeof(CutScene).FullName + "_ForcePredefine");

	public static LocalStorageBool UseSlotDropDownButton = new LocalStorageBool(i_default: true, typeof(CutScene).FullName + "UseSlotDropDownButton");

	public static LocalStorageBool EditCutScene = new LocalStorageBool(i_default: false, typeof(CutScene).FullName + "_EditCutScene");

	public static LocalStorageString CutSceneCode = new LocalStorageString(typeof(CutScene).FullName + "_CutSceneCode");

	public static LocalStorageEnum<FinalizeType> SkipToThisFinalizeType = new LocalStorageEnum<FinalizeType>(FinalizeType.Normal, typeof(CutScene).FullName + "SkipToThisFinalizeType");
}
