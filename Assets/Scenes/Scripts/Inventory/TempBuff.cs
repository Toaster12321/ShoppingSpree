using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TempBuff : MonoBehaviour
{
    public bool activeBuff = false;
    private PlayerCharacter _playerStats;
    private FPSInput _playerSpeed;
    [SerializeField] GameObject dmgBuffImage; //game object for damage buff image
    [SerializeField] GameObject spdBuffImage; //game object for speed buff image

    public enum BuffType { Damage,Speed }

    //event handler
    public event System.Action buffEnd;

    private void Start()
    {
        _playerStats = GetComponent<PlayerCharacter>();
        _playerSpeed = GetComponent<FPSInput>();
    }
    public void startBuff(float duration, BuffType type, float buffAmount)
    {
        //starts buff if buff is not active
        if(type == BuffType.Damage && !activeBuff)
        {
            activeBuff = true;
            dmgBuffImage.SetActive(true);

            StartCoroutine(buffPlayer(duration, type,buffAmount));
        }
        else if(type == BuffType.Speed)
        {
            spdBuffImage.SetActive(true);
        }

        StartCoroutine(buffPlayer(duration, type, buffAmount));

    }

    //coroutine for handling buff timers
    public IEnumerator buffPlayer(float duration, BuffType type, float buffAmount)
    {
        if (type == BuffType.Damage)
        {
            _playerStats.increaseDMG(_playerStats.currentDMG * buffAmount);
        }
        else if (type == BuffType.Speed)
        {
            _playerSpeed.speedBuff(buffAmount, duration);
        }

        yield return new WaitForSeconds(duration);

        if (type == BuffType.Damage)
        {
            //revert damage
            _playerStats.currentDMG = _playerStats.baseDMG;
            dmgBuffImage.SetActive(false);
        }
        else if (type == BuffType.Speed)
        {
            spdBuffImage.SetActive(false);
            _playerSpeed.speedBuff(1f, 0);//resets buff
        }

        activeBuff = false;
        
        //notifies that buff has ended
        if (buffEnd != null)
            buffEnd.Invoke();

    }

}
