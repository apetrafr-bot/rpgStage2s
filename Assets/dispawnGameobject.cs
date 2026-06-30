using System.Collections;
using UnityEngine;
public class dispawnGameobject : MonoBehaviour
{
    public float timeToLife = 0;

    public void Start()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(death());
    }
    public IEnumerator death()
    { 
        yield return new WaitForSeconds(timeToLife);
        Destroy(gameObject);
    }
}
