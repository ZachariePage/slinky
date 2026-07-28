using UnityEngine;
using UnityEngine.EventSystems;

public class SetSelectedGameObjectScript : MonoBehaviour
{
    [SerializeField] GameObject firstSelected;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }
}
