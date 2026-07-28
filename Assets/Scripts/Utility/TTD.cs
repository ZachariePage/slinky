using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTD : MonoBehaviour
{
    // Probably needs renaming to "TTL" since its (usually) time to live 

    [Tooltip("Temps avant de disparaître")]
    [SerializeField] float tempsDeVie = 5;

    float tempsInitial;

    // Just in case 
    [Tooltip("Retourner dans Object Pool après le temps écoulé")]
    [SerializeField] bool isReuseable = true;



    private void OnEnable()
    {
        tempsInitial = Time.time;
    }

    void Update()
    {
        if (Time.time > tempsInitial + tempsDeVie)
        {
            if (isReuseable)
            {
                ObjectPool.ReturnToPool(gameObject);
            }
            else
            {
                Destruction();
            }

        }
    }
    public void Destruction()
    { Destroy(gameObject); }
}
