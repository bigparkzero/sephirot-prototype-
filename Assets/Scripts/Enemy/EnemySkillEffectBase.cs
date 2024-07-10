using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemySkillEffectBase : MonoBehaviour
{
    [HideInInspector] public string skillName;
    [HideInInspector] public EnemySkills skills;
    [HideInInspector] public Animator anim;
    [HideInInspector] public bool isOn;
    [HideInInspector] public float durationTimer;
    [HideInInspector] public EnemyBehavior owner;
    [HideInInspector] public float DURATION;
    
    public GameObject colliderObject;

    protected void Update()
    {
        if (isOn)
        {
            OnPlaying();

            durationTimer -= Time.deltaTime;

            if (durationTimer <= 0)
            {
                durationTimer = 0;
                isOn = false;
                OnExit(false);
            }
        }
    }

    public virtual void OnActivate()
    {
        isOn = true;
        durationTimer = DURATION;
    }

    public virtual void OnPlaying()
    {

    }

    public virtual void OnExit(bool isForcedStop)
    {
        skills.ExitSkill();
        colliderObject.SetActive(false);
    }
}
