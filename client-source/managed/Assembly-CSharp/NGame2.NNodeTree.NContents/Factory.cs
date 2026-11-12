using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NContents.NBattle;
using NGame2.NNodeTree.NContents.NEffect;
using NGame2.NNodeTree.NContents.NExecute;
using NGame2.NNodeTree.NContents.NRoot;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents;

public class Factory : SingletonAuto<Factory>
{
	public HashSet<string> DeprecatedSet = new HashSet<string>();

	public Dictionary<string, Func<NodeData, ContentsBase>> CreateRootDic = new Dictionary<string, Func<NodeData, ContentsBase>>();

	public Dictionary<string, Func<NodeData, ContentsBase>> CreateDic = new Dictionary<string, Func<NodeData, ContentsBase>>();

	protected override bool ThreadSafe => true;

	public static Color GetContentsColor(ContentsBase contents)
	{
		if (contents is CreatureAction)
		{
			return Colors.skyblue;
		}
		if (contents is RootBase)
		{
			return Vespa_Unity_Color_Extension_Method.Lerp(Color.magenta, Color.white, 0.5f);
		}
		if (contents is ExecuteBase)
		{
			return Vespa_Unity_Color_Extension_Method.Lerp(Color.blue, Color.white, 0.5f);
		}
		if (contents is EffectBase)
		{
			return Color.green;
		}
		return Color.white;
	}

	public Factory()
	{
		DeprecatedSet.Add("Effect/CreatureFocusCamera");
		CreateRootDic.Add(ChildRoot.ContentsTypeName, (NodeData i_node) => new ChildRoot(i_node));
		CreateRootDic.Add(BeginFireSkill.ContentsTypeName, (NodeData i_node) => new BeginFireSkill(i_node));
		CreateRootDic.Add(FireSkill.ContentsTypeName, (NodeData i_node) => new FireSkill(i_node));
		CreateRootDic.Add(BeginOperation.ContentsTypeName, (NodeData i_node) => new BeginOperation(i_node));
		CreateRootDic.Add(BeginOperationSingle.ContentsTypeName, (NodeData i_node) => new BeginOperationSingle(i_node));
		CreateRootDic.Add(ReserveOperation.ContentsTypeName, (NodeData i_node) => new ReserveOperation(i_node));
		CreateRootDic.Add(FlyingOperation.ContentsTypeName, (NodeData i_node) => new FlyingOperation(i_node));
		CreateRootDic.Add(OnWidgetMessage.ContentsTypeName, (NodeData i_node) => new OnWidgetMessage(i_node));
		CreateRootDic.Add(OnSpawnCreature.ContentsTypeName, (NodeData i_node) => new OnSpawnCreature(i_node));
		CreateRootDic.Add(OnCreatureEvent.ContentsTypeName, (NodeData i_node) => new OnCreatureEvent(i_node));
		CreateRootDic.Add(OnDamage.ContentsTypeName, (NodeData i_node) => new OnDamage(i_node));
		CreateDic.Add(Calculate.ContentsTypeName, (NodeData i_node) => new Calculate(i_node));
		CreateDic.Add(ReadFromExternal.ContentsTypeName, (NodeData i_node) => new ReadFromExternal(i_node));
		CreateDic.Add(WriteToExternal.ContentsTypeName, (NodeData i_node) => new WriteToExternal(i_node));
		CreateDic.Add(Delay.ContentsTypeName, (NodeData i_node) => new Delay(i_node));
		CreateDic.Add(Wait.ContentsTypeName, (NodeData i_node) => new Wait(i_node));
		CreateDic.Add(UniqueExecute.ContentsTypeName, (NodeData i_node) => new UniqueExecute(i_node));
		CreateDic.Add(ConditionExecute.ContentsTypeName, (NodeData i_node) => new ConditionExecute(i_node));
		CreateDic.Add(IntervalExecute.ContentsTypeName, (NodeData i_node) => new IntervalExecute(i_node));
		CreateDic.Add(MergeExecute.ContentsTypeName, (NodeData i_node) => new MergeExecute(i_node));
		CreateDic.Add(SwitchExecuteUid.ContentsTypeName, (NodeData i_node) => new SwitchExecuteUid(i_node));
		CreateDic.Add(SwitchExecuteString.ContentsTypeName, (NodeData i_node) => new SwitchExecuteString(i_node));
		CreateDic.Add(CreateRuntime.ContentsTypeName, (NodeData i_node) => new CreateRuntime(i_node));
		CreateDic.Add(DisplayBuffIcon.ContentsTypeName, (NodeData i_node) => new DisplayBuffIcon(i_node));
		CreateDic.Add(ObjectTransform.ContentsTypeName, (NodeData i_node) => new ObjectTransform(i_node));
		CreateDic.Add(ObjectApproach.ContentsTypeName, (NodeData i_node) => new ObjectApproach(i_node));
		CreateDic.Add(ObjectDamper.ContentsTypeName, (NodeData i_node) => new ObjectDamper(i_node));
		CreateDic.Add(OnFlyingOperation.ContentsTypeName, (NodeData i_node) => new OnFlyingOperation(i_node));
		CreateDic.Add(OnEndFireSkill.ContentsTypeName, (NodeData i_node) => new OnEndFireSkill(i_node));
		CreateDic.Add(CreatureAction.OldContentsTypeName, (NodeData i_node) => new CreatureAction(i_node));
		CreateDic.Add(CreatureAction.OldContentsTypeName2, (NodeData i_node) => new CreatureAction(i_node));
		CreateDic.Add(CreatureAction.ContentsTypeName, (NodeData i_node) => new CreatureAction(i_node));
		CreateDic.Add(CreatureLoadAction.ContentsTypeName, (NodeData i_node) => new CreatureLoadAction(i_node));
		CreateDic.Add(ActionModifier.ContentsTypeName, (NodeData i_node) => new ActionModifier(i_node));
		CreateDic.Add(ActionModifierList.ContentsTypeName, (NodeData i_node) => new ActionModifierList(i_node));
		CreateDic.Add(SetFunctionality.ContentsTypeName, (NodeData i_node) => new SetFunctionality(i_node));
		CreateDic.Add(PlayCutScene.ContentsTypeName, (NodeData i_node) => new PlayCutScene(i_node));
		CreateDic.Add(PlayVideoPlayer.ContentsTypeName, (NodeData i_node) => new PlayVideoPlayer(i_node));
		CreateDic.Add(OnBeginMoveCallback.OldContentsTypeName, (NodeData i_node) => new OnBeginMoveCallback(i_node));
		CreateDic.Add(OnBeginMoveCallback.ContentsTypeName, (NodeData i_node) => new OnBeginMoveCallback(i_node));
		CreateDic.Add(SendCommandMsg.ContentsTypeName, (NodeData i_node) => new SendCommandMsg(i_node));
		CreateDic.Add(SendCreatureEvent.ContentsTypeName, (NodeData i_node) => new SendCreatureEvent(i_node));
		CreateDic.Add(ForceMoveCreature.ContentsTypeName, (NodeData i_node) => new ForceMoveCreature(i_node));
		CreateDic.Add(SwitchKey.ContentsTypeName, (NodeData i_node) => new SwitchKey(i_node));
		CreateDic.Add(StoreKey.ContentsTypeName, (NodeData i_node) => new StoreKey(i_node));
		CreateDic.Add(AssetEventCamera.ContentsTypeName, (NodeData i_node) => new AssetEventCamera(i_node));
		CreateDic.Add(ModifyEventCamera2.ContentsTypeName, (NodeData i_node) => new ModifyEventCamera2(i_node));
		CreateDic.Add(CameraPostProcess.ContentsTypeName, (NodeData i_node) => new CameraPostProcess(i_node));
		CreateDic.Add(CameraShakeDetail.ContentsTypeName, (NodeData i_node) => new CameraShakeDetail(i_node));
		CreateDic.Add(CameraShakeSimple.ContentsTypeName, (NodeData i_node) => new CameraShakeSimple(i_node));
		CreateDic.Add(CreateAssetEffect.ContentsTypeName, (NodeData i_node) => new CreateAssetEffect(i_node));
		CreateDic.Add(CreateSkillUiEffect.ContentsTypeName, (NodeData i_node) => new CreateSkillUiEffect(i_node));
		CreateDic.Add(SpawnRenderCreature.ContentsTypeName, (NodeData i_node) => new SpawnRenderCreature(i_node));
		CreateDic.Add(SpawnRenderCreature2.ContentsTypeName, (NodeData i_node) => new SpawnRenderCreature2(i_node));
		CreateDic.Add(SpawnAssetEffect.ContentsTypeName, (NodeData i_node) => new SpawnAssetEffect(i_node));
		CreateDic.Add(ReleaseAssetEffect.ContentsTypeName, (NodeData i_node) => new ReleaseAssetEffect(i_node));
		CreateDic.Add(LoadBattleField.ContentsTypeName, (NodeData i_node) => new LoadBattleField(i_node));
		CreateDic.Add(CreateAssetSound.ContentsTypeName, (NodeData i_node) => new CreateAssetSound(i_node));
		CreateDic.Add(CreateScreenEffect.ContentsTypeName, (NodeData i_node) => new CreateScreenEffect(i_node));
		CreateDic.Add(CreateScreenUIEffect.ContentsTypeName, (NodeData i_node) => new CreateScreenUIEffect(i_node));
		CreateDic.Add(CreatePivot.ContentsTypeName, (NodeData i_node) => new CreatePivot(i_node));
		CreateDic.Add(CreateModel.ContentsTypeName, (NodeData i_node) => new CreateModel(i_node));
		CreateDic.Add(CreateParticle.ContentsTypeName, (NodeData i_node) => new CreateParticle(i_node));
		CreateDic.Add(CreateTrail.ContentsTypeName, (NodeData i_node) => new CreateTrail(i_node));
		CreateDic.Add(CreatureColor.ContentsTypeName, (NodeData i_node) => new CreatureColor(i_node));
		CreateDic.Add(CreatureAlpha.ContentsTypeName, (NodeData i_node) => new CreatureAlpha(i_node));
		CreateDic.Add(CreatureScale.ContentsTypeName, (NodeData i_node) => new CreatureScale(i_node));
		CreateDic.Add(CreatureCasting.ContentsTypeName, (NodeData i_node) => new CreatureCasting(i_node));
		CreateDic.Add(CreatureMainTex.ContentsTypeName, (NodeData i_node) => new CreatureMainTex(i_node));
		CreateDic.Add(CreatureMaterialModify.ContentsTypeName, (NodeData i_node) => new CreatureMaterialModify(i_node));
		CreateDic.Add(CreatureMaterialModifyFloat.ContentsTypeName, (NodeData i_node) => new CreatureMaterialModifyFloat(i_node));
		CreateDic.Add(PlaySoundDirect.ContentsTypeName, (NodeData i_node) => new PlaySoundDirect(i_node));
		CreateDic.Add(PlayBgm.ContentsTypeName, (NodeData i_node) => new PlayBgm(i_node));
		CreateDic.Add(ScreenFilterEffect.ContentsTypeName, (NodeData i_node) => new ScreenFilterEffect(i_node));
		CreateDic.Add(TimeScale.ContentsTypeName, (NodeData i_node) => new TimeScale(i_node));
		CreateDic.Add(FieldCameraAction.ContentsTypeName, (NodeData i_node) => new FieldCameraAction(i_node));
		CreateDic.Add(CreateBattleCreature.ContentsTypeName, (NodeData i_node) => new CreateBattleCreature(i_node));
		CreateDic.Add(DestroyBattleCreature.ContentsTypeName, (NodeData i_node) => new DestroyBattleCreature(i_node));
		CreateDic.Add(FindBattleCreature.ContentsTypeName, (NodeData i_node) => new FindBattleCreature(i_node));
		CreateDic.Add(MoveToPosition.ContentsTypeName, (NodeData i_node) => new MoveToPosition(i_node));
		CreateDic.Add(FireEventSkill.ContentsTypeName, (NodeData i_node) => new FireEventSkill(i_node));
		CreateDic.Add(ModifyBattleScene.ContentsTypeName, (NodeData i_node) => new ModifyBattleScene(i_node));
		CreateDic.Add(ModifyCreatureModel.ContentsTypeName, (NodeData i_node) => new ModifyCreatureModel(i_node));
		CreateDic.Add(ModifyWeaponPrefix.ContentsTypeName, (NodeData i_node) => new ModifyWeaponPrefix(i_node));
		CreateDic.Add(FocusBattleCreature.ContentsTypeName, (NodeData i_node) => new FocusBattleCreature(i_node));
		CreateDic.Add(ToggleBattleUi.ContentsTypeName, (NodeData i_node) => new ToggleBattleUi(i_node));
		CreateDic.Add(ToggleBattleUi2.ContentsTypeName, (NodeData i_node) => new ToggleBattleUi2(i_node));
		CreateDic.Add(IsActiveOperation.ContentsTypeName, (NodeData i_node) => new IsActiveOperation(i_node));
		CreateDic.Add(SubCreatureAlpha.ContentsTypeName, (NodeData i_node) => new SubCreatureAlpha(i_node));
		CreateDic.Add(CreateSubCreature.ContentsTypeName, (NodeData i_node) => new CreateSubCreature(i_node));
		CreateDic.Add(DestroySubCreature.ContentsTypeName, (NodeData i_node) => new DestroySubCreature(i_node));
		CreateDic.Add(SubCreatureAction.ContentsTypeName, (NodeData i_node) => new SubCreatureAction(i_node));
		CreateDic.Add(FindFieldMapCreature.ContentsTypeName, (NodeData i_node) => new FindFieldMapCreature(i_node));
		CreateDic.Add(FieldMapCreatureAction.ContentsTypeName, (NodeData i_node) => new FieldMapCreatureAction(i_node));
	}

	public ContentsBase CreateFromJson(JsonData i_json, NodeData i_owner)
	{
		if (!i_json.IsObject())
		{
			return null;
		}
		JsonData jsonData = i_json["type"];
		if (!jsonData.IsString())
		{
			return null;
		}
		Func<NodeData, ContentsBase> createFunc = GetCreateFunc(jsonData.valueStr);
		if (createFunc == null)
		{
			DeprecatedSet.Contains(jsonData.valueStr);
			return null;
		}
		ContentsBase contentsBase = createFunc(i_owner);
		if (contentsBase == null)
		{
			return null;
		}
		contentsBase.LoadFromJson(i_json);
		return contentsBase;
	}

	public Func<NodeData, ContentsBase> GetCreateFunc(string i_typeName)
	{
		Func<NodeData, ContentsBase> vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(CreateRootDic, i_typeName, null);
		if (vALUE != null)
		{
			return vALUE;
		}
		vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(CreateDic, i_typeName, null);
		if (vALUE != null)
		{
			return vALUE;
		}
		return null;
	}
}
