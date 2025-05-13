using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPrefabPlacerMode
{
    public abstract void OnModeActivated();

    public void OnModeDeactivated();
}
