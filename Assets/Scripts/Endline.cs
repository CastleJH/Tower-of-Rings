using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endline : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Triggered");
        if (collision.tag == "Monster")
        {
            Debug.Log("Monster!");
            BattleManager.instance.isBattleOver = true;
        }
    }
}