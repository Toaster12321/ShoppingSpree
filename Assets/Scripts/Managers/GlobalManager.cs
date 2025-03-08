using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ensures other managers exist
[RequireComponent(typeof(SoundFXManager))]

public class GlobalManager : MonoBehaviour
{
    //for other code to access managers
    public static SoundFXManager Audio { get; private set; }

    private List<IGameManager> startSequence;

    private void Awake()
    {
        //gathering scripts
        Audio = GetComponent<SoundFXManager>();

        startSequence = new List<IGameManager>();
        startSequence.Add(Audio);

        //launch startup sequence
        StartCoroutine(StartupManagers());
    }


    private IEnumerator StartupManagers()
    {
        //collects all managers for startup
        foreach (IGameManager managers in startSequence)
        {
            managers.Startup();
        }

        yield return null;

        int numModules = startSequence.Count;
        int numReady = 0;

        //while the number of modules ready is less than the total number of modules
        while (numReady < numModules)
        {
            int lastReady = numReady;
            numReady = 0;

            foreach (IGameManager manager in startSequence) // for each module if its ready increment number ready
            {
                if (manager.status == ManagerStatus.Started)
                {
                    numReady++;
                }
            }

            if (numReady > lastReady)
            {
                Debug.Log($"Progress: {numReady}/{numModules}");
            }

            yield return null;
        }

        Debug.Log("All managers started up");

    }
}