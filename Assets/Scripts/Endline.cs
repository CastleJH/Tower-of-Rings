using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endline : MonoBehaviour
{
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();    
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            if (DeckManager.instance.isAngelEffect)
            {
                DeckManager.instance.isAngelEffect = false;
                DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.angelRing);
                for (int i = 0; i < BattleManager.instance.monsters.Count; i++)
                    BattleManager.instance.monsters[i].AE_Angel();
            }
            else
            {
                Monster monster = collision.GetComponent<Monster>();
                GameManager.instance.ChangePlayerCurHP(-monster.baseMonster.baseATK);
                anim.SetTrigger("isAttacked");
                monster.RemoveFromBattle(false);
            }
        }
    }
}