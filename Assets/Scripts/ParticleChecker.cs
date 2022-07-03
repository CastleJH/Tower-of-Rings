using System.Collections;
using System.Collections.Generic;
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
        id = int.Parse((gameObject.name.Split(' ')[1]).Split('(')[0]);
    }

    void OnEnable()
    {
        particle.Play();
    }

    void OnDisable()
    {
        GameManager.instance.ReturnParticleToPool(this, id);
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
        gameObject.SetActive(true);
    }

    //��ƼŬ�� �����Ѵ�.
    public void StopParticle()
    {
        particle.Stop(true);
        gameObject.SetActive(false);
    }
}
