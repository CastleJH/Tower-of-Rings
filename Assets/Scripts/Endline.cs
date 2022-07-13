using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endline : MonoBehaviour
{
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
                if (monster.IsNormalMonster()) GameManager.instance.ChangePlayerCurHP(-1);
                else GameManager.instance.ChangePlayerCurHP(-20);
                monster.RemoveFromBattle(0.0f);
            }
        }
    }
}