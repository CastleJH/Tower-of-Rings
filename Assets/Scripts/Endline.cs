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
        if (collision.tag == "Monster")     //몬스터가 엔드라인에 닿았다면
        {
            if (DeckManager.instance.isAngelEffect)     //천사링의 효과가 유효하다면 천사링을 제거하고 모든 몬스터에 효과를 적용한다.
            {
                DeckManager.instance.isAngelEffect = false;
                DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.angelRing);
                for (int i = 0; i < BattleManager.instance.monsters.Count; i++)
                {
                    BattleManager.instance.monsters[i].PlayParticleCollision(27, 0.0f);
                    BattleManager.instance.monsters[i].AE_Angel();
                }
            }
            else    //그렇지 않다면 HP를 몬스터의 공격력만큼 감소시키고 애니메이션을 적용한 뒤 몬스터를 제거한다.
            {
                Monster monster = collision.GetComponent<Monster>();
                GameManager.instance.ChangePlayerCurHP(-monster.baseMonster.baseATK);
                anim.SetTrigger("isAttacked");
                monster.RemoveFromBattle(false);
            }
        }
    }
}