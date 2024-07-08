using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaintBloom_NormalAttack : EnemySkillEffectBase
{
    Collider col;
    float duration;
    public override void OnActivate()
    {
        base.OnActivate();

        col.gameObject.SetActive(true);
        print("이펙트 재생");
        print("콜라이더 on");
    }

    public override void OnPlaying()
    {
        base.OnPlaying();

        duration -= Time.deltaTime;

        print("특별한 효과");
    }

    public override void OnExit(bool isForcedStop)
    {
        base.OnExit(isForcedStop);

        col.gameObject.SetActive(false);
        print("이펙트, 콜라이더 끄기");
    }
}
