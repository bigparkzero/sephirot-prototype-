using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    float hp;
    public float maxHP = -1;

    public Slider slider_hpBar;

    public float HP
    {
        get
        {
            return hp;
        }
    }

    public bool IsDead
    {
        get
        {
            return hp <= 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (maxHP < 1)
        {
            throw new System.Exception("Max HP can not be below 1. Please check inspector values of this GameObject(" + gameObject.name + ")'s Component(" + typeof(Stats).Name + ")");
        }

        hp = maxHP;
        UpdateUI();
    }

    public void Healed(float heal)
    {
        if (IsDead) return;

        hp = Mathf.Min(hp + heal, maxHP);
        UpdateUI();
    }

    public void Damaged(float damage)
    {
        if (IsDead) return;

        hp = Mathf.Max(0, hp - damage);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (slider_hpBar != null)
        {
            slider_hpBar.value = hp / maxHP;
        }
    }
}
