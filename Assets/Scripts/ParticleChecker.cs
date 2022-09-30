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

    //enable되면 자동으로 플레이
    void OnEnable()
    {
        particle.Play();
    }

    //풀에 되돌린다. duration 후 자동으로 불림.
    void InvokeReturnParticle()
    {
        particle.Stop();
        if (gameObject.name[0] == 'c') GameManager.instance.ReturnParticleToPool(this, id);
        else GameManager.instance.ReturnMonsterParticleToPool(this, id);
    }

    //파티클을 플레이한다. duration을 0.0f로 주면 원래 duration을 쓴다.
    public void PlayParticle(Transform _parent, float _duration)
    {
        //위치를 설정한다.
        transform.position = new Vector3(_parent.position.x, _parent.position.y, -0.1f);
        transform.parent = _parent;

        if (_duration != 0.0f)  //지속 시간을 새로 지정해야 하는 경우
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

    //파티클을 위치 조정 후 플레이한다.
    public void PlayParticle(Transform _parent, float _duration, Vector3 move)
    {
        //위치를 설정한다.
        transform.position = _parent.position + move;
        transform.parent = _parent;

        Invoke("InvokeReturnParticle", _duration);
        gameObject.SetActive(true);
    }

    //파티클을 위치에서 고정하여 플레이한다.
    public void PlayParticle(Vector3 position, float _duration)
    {
        //위치를 설정한다.
        transform.position = position;

        Invoke("InvokeReturnParticle", _duration);
        gameObject.SetActive(true);
    }

    //파티클을 정지한다.
    public void StopParticle()
    {
        particle.Stop(true);
        gameObject.SetActive(false);
    }
}
