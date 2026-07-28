using UnityEngine;
using UnityEngine.UI;

public class SettingsTabNavFixer : MonoBehaviour
{
    [SerializeField] private Selectable[] tabButtons;
    [SerializeField] private Selectable firstElement;

    private void OnEnable()
    {
        foreach (Selectable tab in tabButtons)
        {
            Navigation nav = tab.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnDown = firstElement;
            tab.navigation = nav;
        }
    }
}
