using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    [SerializeField] private Text KillLogText;
    
    public void Init(string killer, string killed)
    {
        KillLogText = transform.GetChild(1).GetComponent<Text>();
        KillLogText.text = $"<b>{killer}</b> killed <b>{killed}</b>";
        Destroy(gameObject, 4f);
    }
}
