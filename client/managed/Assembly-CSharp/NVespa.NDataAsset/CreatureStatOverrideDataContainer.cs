using UnityEngine;

namespace NVespa.NDataAsset;

public class CreatureStatOverrideDataContainer : IndexableDataAsset<int, CreatureStatOverrideData>
{
	[Tooltip("사용자 영웅을 제외한 크리쳐의 스탯을 강제로 수정하는 테이블입니다. 이름은 HeroIndex 이지만 모든 크리쳐는 HeroIndex로 종류를 구분합니다. 해당 스탯에 먼저 Mult / 1000을 곱하고 Add를 더합니다. 매 전투마다 변경사항이 자동 적용됩니다. 한 크리쳐의 여러 스탯을 바꾸려면 같은 인덱스로 여러번 정의하세요.")]
	public bool DescriptionToolTip;

	protected override bool AlwaysReloadAsset => true;

	protected override int GetKey(CreatureStatOverrideData d)
	{
		return d.HeroIndex;
	}
}
