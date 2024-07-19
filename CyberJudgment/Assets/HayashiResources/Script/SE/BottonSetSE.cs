using UnityEngine;
using UnityEngine.UI;
public class BottonSetSE : MonoBehaviour
{
    [SerializeField, Header("SE��")]
    private string _seName;
    [SerializeField, Header("����")]
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
            Debug.LogError("�{�^���R���|�[�l���g����������");
        }
    }
}
