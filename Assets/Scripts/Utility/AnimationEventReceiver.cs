using UnityEngine;

[System.Serializable]
public class CueEntry
{
    public string id;
    public GameCue cue;
}

public class AnimationEventReceiver : MonoBehaviour
{
    public CueEntry[] cues;

    public void TriggerCueByID(string id)
    {
        foreach (var entry in cues)
        {
            if (entry.id == id)
            {
                entry.cue?.Execute(transform.position);
                return;
            }
        }

        Debug.LogWarning($"Cue ID '{id}' not found on {gameObject.name}");
    }
    
    public void TriggerCue(GameCue cue)
    {
        if (cue == null)
        {
            Debug.LogWarning($"Missing cue on {gameObject.name}");
            return;
        }

        cue.Execute(transform.position);
    }
    public void TriggerSpriteCue(GameCue cue, Sprite sprite)
    {
        if (cue == null)
        {
            Debug.LogWarning($"Missing cue on {gameObject.name}");
            return;
        }

        cue.Execute(transform.position, sprite);
    }
}
