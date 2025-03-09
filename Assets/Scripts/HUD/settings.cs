using UnityEngine;

public class settings : MonoBehaviour
{
    //shows settings menu
    public void Open()
    {
        gameObject.SetActive(true);
    }

    //closes settings menu
    public void Close()
    {
        gameObject.SetActive(false);
    }

}
