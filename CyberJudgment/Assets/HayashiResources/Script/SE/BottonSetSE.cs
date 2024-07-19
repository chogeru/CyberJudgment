using UnityEngine;
using UnityEngine.UI;
public class BottonSetSE : MonoBehaviour
{
    [SerializeField, Header("SE名")]
    private string _seName;
    [SerializeField, Header("音量")]
    private float _volume;
    void Start()
    {
        Button button = this.gameObject.GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(() => SEManager.instance.PlaySound(_seName, _volume));
        }
        else
        {
            Debug.LogError("ボタンコンポーネント無いっすよ");
        }
    }
}
