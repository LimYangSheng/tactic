using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private int maxHP;
    public GameObject fill;

    public void Setup(int hp)
    { 
        maxHP = hp;
        UpdateHealthBar(maxHP);
    }

    public void UpdateHealthBar(int currentHP)
    { 
        fill.transform.localScale = new Vector3((float)currentHP/(float)maxHP, 1.0f, 1.0f);
    }
}
