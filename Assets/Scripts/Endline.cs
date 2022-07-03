using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endline : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            BattleManager.instance.isBattleOver = true;
        }
    }
}