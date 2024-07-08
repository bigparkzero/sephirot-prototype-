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
        print("����Ʈ ���");
        print("�ݶ��̴� on");
    }

    public override void OnPlaying()
    {
        base.OnPlaying();

        duration -= Time.deltaTime;

        print("Ư���� ȿ��");
    }

    public override void OnExit(bool isForcedStop)
    {
        base.OnExit(isForcedStop);

        col.gameObject.SetActive(false);
        print("����Ʈ, �ݶ��̴� ����");
    }
}
