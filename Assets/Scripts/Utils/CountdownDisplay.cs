using UnityEngine;
using TMPro;

public class CountdownDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private CountdownTimer timer;

    void Start()
    {
        timer = CountdownTimer.Instance;

        timer.OnTimerStart += HandleStart;
        timer.OnTimerTick += HandleTick;
        timer.OnTimerComplete += HandleComplete;
        timer.OnTimerCancel += HandleCancel;

        text.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (timer == null) return;

        timer.OnTimerStart -= HandleStart;
        timer.OnTimerTick -= HandleTick;
        timer.OnTimerComplete -= HandleComplete;
        timer.OnTimerCancel -= HandleCancel;
    }

    void HandleStart(int time)
    {
        text.gameObject.SetActive(true);
        text.text = time.ToString();
    }

    void HandleTick(int time)
    {
        text.text = time > 0 ? time.ToString() : "";
        text.gameObject.GetComponent<Animator>().SetTrigger("Tick");
    }

    void HandleComplete()
    {
        Invoke(nameof(Hide), 0.5f);
    }

    void HandleCancel()
    {
        Hide();
    }

    void Hide()
    {
        text.gameObject.SetActive(false);
    }
}