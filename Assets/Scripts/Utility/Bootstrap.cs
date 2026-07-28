using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        if (FindObjectOfType<BootstrapToken>() == null)
        {
            SceneManager.LoadScene("Bootstrap", LoadSceneMode.Additive);
        }
    }
}
