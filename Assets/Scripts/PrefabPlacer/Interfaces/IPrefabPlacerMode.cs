using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IPrefabPlacerMode
{
    abstract void OnModeActivated(ProfilePlacedObjectsTrackerSO trackerSO, string sceneName);

    abstract void OnModeDeactivated();

    abstract void OnActionDone();

}
