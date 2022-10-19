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
        if (collision.tag == "Monster")     //���Ͱ� ������ο� ��Ҵٸ�
        {
            if (DeckManager.instance.isAngelEffect)     //õ�縵�� ȿ���� ��ȿ�ϴٸ� õ�縵�� �����ϰ� ��� ���Ϳ� ȿ���� �����Ѵ�.
            {
                DeckManager.instance.isAngelEffect = false;
                DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.angelRing);
                for (int i = 0; i < BattleManager.instance.monsters.Count; i++)
                {
                    BattleManager.instance.monsters[i].PlayParticleCollision(27, 0.0f);
                    BattleManager.instance.monsters[i].AE_Angel();
                }
            }
            else    //�׷��� �ʴٸ� HP�� ������ ���ݷ¸�ŭ ���ҽ�Ű�� �ִϸ��̼��� ������ �� ���͸� �����Ѵ�.
            {
                Monster monster = collision.GetComponent<Monster>();
                if (monster.baseMonster.tier == 'n' || monster.baseMonster.tier == 's') GameManager.instance.ChangePlayerCurHP(-1);
                else GameManager.instance.ChangePlayerCurHP(-10);
                anim.SetTrigger("isAttacked");
                BattleManager.instance.audioSource.PlayOneShot(GameManager.instance.specialAudios[5]);
                monster.RemoveFromBattle(false);
            }
        }
    }
}