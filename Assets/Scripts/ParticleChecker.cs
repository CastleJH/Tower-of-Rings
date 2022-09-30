using UnityEditorInternal;
using UnityEngine;

public class ParticleChecker : MonoBehaviour
{
    public int id;
    ParticleSystem particle;
    ParticleSystem[] particles;
    
    void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        particles = GetComponentsInChildren<ParticleSystem>();
        id = int.Parse(gameObject.name.Split(' ')[1].Split('(')[0]);
    }

    //enable�Ǹ� �ڵ����� �÷���
    void OnEnable()
    {
        particle.Play();
    }

    //Ǯ�� �ǵ�����. duration �� �ڵ����� �Ҹ�.
    void InvokeReturnParticle()
    {
        particle.Stop();
        if (gameObject.name[0] == 'c') GameManager.instance.ReturnParticleToPool(this, id);
        else GameManager.instance.ReturnMonsterParticleToPool(this, id);
    }

    //��ƼŬ�� �÷����Ѵ�. duration�� 0.0f�� �ָ� ���� duration�� ����.
    public void PlayParticle(Transform _parent, float _duration)
    {
        //��ġ�� �����Ѵ�.
        transform.position = new Vector3(_parent.position.x, _parent.position.y, -0.1f);
        transform.parent = _parent;

        if (_duration != 0.0f)  //���� �ð��� ���� �����ؾ� �ϴ� ���
        {
            particle.Stop(true);

            for (int i = 0; i < particles.Length; i++)
            {
                var main = particles[i].main;
                main.duration = _duration;
            }
        }
        Invoke("InvokeReturnParticle", particle.main.duration);
        gameObject.SetActive(true);
    }

    //��ƼŬ�� ��ġ ���� �� �÷����Ѵ�.
    public void PlayParticle(Transform _parent, float _duration, Vector3 move)
    {
        //��ġ�� �����Ѵ�.
        transform.position = _parent.position + move;
        transform.parent = _parent;

        Invoke("InvokeReturnParticle", _duration);
        gameObject.SetActive(true);
    }

    //��ƼŬ�� ��ġ���� �����Ͽ� �÷����Ѵ�.
    public void PlayParticle(Vector3 position, float _duration)
    {
        //��ġ�� �����Ѵ�.
        transform.position = position;

        Invoke("InvokeReturnParticle", _duration);
        gameObject.SetActive(true);
    }

    //��ƼŬ�� �����Ѵ�.
    public void StopParticle()
    {
        particle.Stop(true);
        gameObject.SetActive(false);
    }
}
