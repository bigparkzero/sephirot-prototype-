using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnemySkill
{
    public string skillName;

    public GameObject effectObject;

    [HideInInspector] public float cooldownTimer;
    [InspectorName("Skill Cooldown")] public float baseCooldown;

    public float range;
    public float DURATION = -1;
}

public class EnemySkills : MonoBehaviour
{
    EnemyBehavior behavior;

    [SerializeField]
    EnemySkill[] skills;

    public Dictionary<string, List<EnemySkill>> dic_skills = new();

    [HideInInspector] public float globalCooldownTimer;
    [InspectorName("Global Cooldown")] public float baseGlobalCooldown = 5f;

    string lastSkill = "";
    int skillIndex = 0;

    string selectedSkill;

    // Start is called before the first frame update
    void Start()
    {
        behavior = GetComponent<EnemyBehavior>();

        Animator anim = GetComponent<Animator>();

        foreach (EnemySkill skill in skills)
        {
            if (skill.DURATION <= 0)
            {
                throw new Exception("the 'DURATION' variable in EnemySkill of EnemySkills component must be greater than 0. Please check inspector values of this GameObject(" + gameObject.name + ")");
            }
            if (skill.effectObject.TryGetComponent(out EnemySkillEffectBase effect))
            {
                effect.skillName = skill.skillName;
                effect.skills = this;
                effect.anim = anim;
                effect.DURATION = skill.DURATION;
                effect.owner = gameObject;
            }
            else
            {
                throw new System.Exception("There is no EnemySkillEffectBase on the effect object of skill: +" + skill.skillName + "! Skill effects must have EnemySkillEffectBase component.");
            }

            if (!dic_skills.ContainsKey(skill.skillName))
            {
                dic_skills.Add(skill.skillName, new List<EnemySkill>());
            }

            dic_skills[skill.skillName].Add(skill);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (globalCooldownTimer > 0)
        {
            globalCooldownTimer -= Time.deltaTime;
        }

        foreach (EnemySkill skill in skills)
        {
            if (skill.cooldownTimer > 0)
            {
                skill.cooldownTimer -= Time.deltaTime;
            }
        }
    }

    public bool PrepareSkill()
    {
        if (globalCooldownTimer > 0) return false;

        float distance = Vector3.Distance(behavior.target.transform.position, transform.position);

        List<string> availableSkills = dic_skills.Values
            .SelectMany(skillList => skillList)
            .Where(skill => skill.cooldownTimer <= 0 && distance <= skill.range)
            .Select(skill => skill.skillName)
            .ToList();

        if (availableSkills.Count <= 0) return false;

        List<string> otherSkills = availableSkills.Where(skillName => skillName != lastSkill).ToList();

        if (otherSkills.Count > 0)
        {
            selectedSkill = otherSkills[UnityEngine.Random.Range(0, otherSkills.Count)];
        }
        else if (availableSkills.Contains(lastSkill))
        {
            selectedSkill = lastSkill;
        }
        else
        {
            selectedSkill = null;
        }

        if (selectedSkill == null || selectedSkill.Length <= 0)
        {
            return false;
        }

        return true;
    }

    public void UseSkill()
    {
        globalCooldownTimer = baseGlobalCooldown;

        EnemySkill skillToActivate = dic_skills[selectedSkill].First();
        skillToActivate.cooldownTimer = skillToActivate.baseCooldown;
        lastSkill = selectedSkill;
        skillIndex = 0;

        if (skillToActivate.effectObject.TryGetComponent(out EnemySkillEffectBase effect))
        {
            effect.OnActivate();
        }
        else
        {
            throw new System.Exception("There is no EnemySkillEffectBase on the effect object of the skill: +" + selectedSkill + "! Skill effects must have EnemySkillEffectBase component.");
        }
    }

    public void ActivateSkillChain()
    {
        skillIndex++;
        EnemySkill skillToActivate = dic_skills[lastSkill][skillIndex];

        if (skillToActivate == null) throw new System.Exception("No chain skill :" + lastSkill + ", index : " + skillIndex);

        if (skillToActivate.effectObject.TryGetComponent(out EnemySkillEffectBase effect))
        {
            effect.OnActivate();
        }
        else
        {
            throw new System.Exception("There is no EnemySkillEffectBase on the effect object of the skill: +" + lastSkill + ", index: " + skillIndex + "! Skill effects must have EnemySkillEffectBase component.");
        }
    }

    public void ExitSkill()
    {
        behavior.EndAttack();
    }

    public void StopSkill()
    {
        EnemySkill currentSkill = dic_skills[lastSkill][skillIndex];

        if (currentSkill.effectObject.TryGetComponent(out EnemySkillEffectBase effect))
        {
            effect.isOn = false;
            effect.durationTimer = 0;
            effect.OnExit(true);
        }
        else
        {
            throw new System.Exception("There is no EnemySkillEffectBase on the effect object of skill: +" + lastSkill + ", index: " + skillIndex + "! Skill effects must have EnemySkillEffectBase component.");
        }
    }
}