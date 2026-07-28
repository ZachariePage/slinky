using UnityEngine;

public interface IActivable
{
    public void Activate();
    public void Deactivate();

    public bool ActivateMessage();

    public bool IsActivated();
    public bool CanBeExternallyActivated();
}
