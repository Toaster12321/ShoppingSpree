using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TempBuff : MonoBehaviour
{
    public bool activeBuff = false;
    private PlayerCharacter _playerStats;
    [SerializeField] GameObject dmgBuffImage; //game object for damage buff image
    [SerializeField] GameObject spdBuffImage; //game object for speed buff image

    //event handler
    public event System.Action buffEnd;

    private void Start()
    {
        _playerStats = GetComponent<PlayerCharacter>();
    }
    public void startBuff(float duration)
    {
        //starts buff if buff is not active
        if(!activeBuff)
        {
            activeBuff = true;
            dmgBuffImage.SetActive(true);
            StartCoroutine(buffPlayer(duration));
        }

    }

    public IEnumerator buffPlayer(float duration)
    {
  
        yield return new WaitForSeconds(duration);
        activeBuff = false;
        dmgBuffImage.SetActive(false);

        //notifies that buff has ended
        if (buffEnd != null)
            buffEnd.Invoke();

    }

}
