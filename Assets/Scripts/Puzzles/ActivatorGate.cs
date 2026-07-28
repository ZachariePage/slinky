using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActivatorGate : Activator
{
    public GameObject gate;

    [SerializeField] protected GameCue[] activationCues;
    [SerializeField] private Transform activationCuesLocation;

    public List<GameObject> objsToDisable;
    public List<GameObject> objsToEnable;
    public List<GameObject> objsToEnableAnim;

    [Header("Auto Revert")]
    [SerializeField] private bool revertAfterSeconds = false;
    [SerializeField] private float revertDelay = 1f;

    private Dictionary<GameObject, GameObject> originalTemplates = new();
    private List<GameObject> spawnedObjects = new();

    protected override void Start()
    {
        base.Start();

        if (activationCuesLocation == null)
            activationCuesLocation = transform;

        // Create hidden templates for reset spawning
        foreach (var obj in objsToDisable)
        {
            if (obj == null) continue;

            GameObject template = Instantiate(obj, obj.transform.position, obj.transform.rotation, obj.transform.parent);
            template.SetActive(false);
            originalTemplates[obj] = template;
        }
    }

    protected override void Completion()
    {
        base.Completion();

        foreach (GameObject obj in objsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (GameObject obj in objsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        foreach (GameObject obj in objsToEnableAnim)
        {
            if (obj != null)
            {
                obj.SetActive(true);

                Animator objAnim = obj.GetComponent<Animator>();
                if (objAnim != null)
                {
                    objAnim.enabled = true;
                }
            }
        }
        
        if (objsToDisable.Count <= 0)
        {
            OnDoorDestroy();

            if (gate != null)
                Destroy(gate);
        }
        
        if (revertAfterSeconds && completed)
            StartCoroutine(RevertAfterDelay());
    }

    private IEnumerator RevertAfterDelay()
    {

        yield return new WaitForSeconds(revertDelay);
        RevertCompletion();
    }

    private void RevertCompletion()
    {
        //foreach (ActivationMechanism condition in conditions)
        //{
        //    if (condition == null)
        //    {
        //        continue;
        //    }
        //    condition.Deactivate();
        //}

        Array.Clear(conditions, 0, conditions.Length);
        completed = false;

        // Destroy previously spawned reset objects
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        foreach (var obj in objsToDisable)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
        objsToDisable.Clear();

        // Respawn fresh copies
        int arrayNum = 0;
        foreach (var kvp in originalTemplates)
        {
            GameObject template = kvp.Value;

            if (template == null) continue;

            GameObject newObj = Instantiate(
                template,
                template.transform.position,
                template.transform.rotation,
                template.transform.parent
            );

            newObj.SetActive(true);
            spawnedObjects.Add(newObj);
            objsToDisable.Add(newObj);
            ActivationMechanism activationMechanism = newObj.GetComponent<ActivationMechanism>();

            conditions[arrayNum] = activationMechanism;
            arrayNum++;

        }

        foreach (GameObject obj in objsToEnable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void OnDoorDestroy()
    {
        foreach (GameCue cue in activationCues)
        {
            if (gate != null)
                cue?.Execute(gate.transform.position);
            else
                cue?.Execute(activationCuesLocation.position);
        }
    }
}