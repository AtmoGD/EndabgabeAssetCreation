using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcController : MonoBehaviour
{
    float speed;
    float lifeTime;
    bool started = false;
    void Update()
    {
        if(!started) return;

        transform.position = transform.position + (transform.forward * speed * Time.deltaTime);

        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0f)
            Die();
    }

    void Die() {
        Destroy(this.gameObject);
    }

    public void Init(float _speed, float _lifetime) {
        speed = _speed;
        lifeTime = _lifetime;
        started = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("EnemyArc")) {
            other.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
        }

        Die();
    }
}
