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
}

[RequireComponent(typeof(EnemyState))]
public class EnemySkills : MonoBehaviour
{
    EnemyState state;

    [SerializeField]
    EnemySkill[] skills;

    public Dictionary<string, List<EnemySkill>> dic_skills = new();

    [HideInInspector] public float globalCooldownTimer;
    [InspectorName("Global Cooldown")] public float baseGlobalCooldown = 5f;

    string lastSkill = "";
    int skillIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        state = GetComponent<EnemyState>();

        Animator anim = GetComponent<Animator>();

        foreach (EnemySkill skill in skills)
        {
            if (skill.effectObject.TryGetComponent(out EnemySkillEffectBase effect))
            {
                effect.skillName = skill.skillName;
                effect.skills = this;
                effect.anim = anim;
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

    public bool ActivateSkill()
    {
        if (globalCooldownTimer > 0) return false;

        List<string> availableSkills = dic_skills.Values
            .SelectMany(skillList => skillList)
            .Where(skill => skill.cooldownTimer <= 0)
            .Select(skill => skill.skillName)
            .ToList();

        if (availableSkills.Count <= 0) return false;

        List<string> otherSkills = availableSkills.Where(skillName => skillName != lastSkill).ToList();

        string selectedSkill = null;
        if (otherSkills.Count > 0)
        {
            selectedSkill = otherSkills[UnityEngine.Random.Range(0, otherSkills.Count)];
        }
        else if (availableSkills.Contains(lastSkill))
        {
            selectedSkill = lastSkill;
        }

        if (selectedSkill == null || selectedSkill.Length <= 0)
        {
            return false;
        }

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
            throw new System.Exception("There is no EnemySkillEffectBase on the effect object of skill: +" + selectedSkill + "! Skill effects must have EnemySkillEffectBase component.");
        }

        return true;
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
            throw new System.Exception("There is no EnemySkillEffectBase on the effect object of skill: +" + lastSkill + ", index: " + skillIndex + "! Skill effects must have EnemySkillEffectBase component.");
        }
    }

    public void ExitSkill()
    {
        state.ExitSkill();
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