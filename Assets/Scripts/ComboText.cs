using TMPro;
using UnityEngine;

public class ComboText : MonoBehaviour
{
    public float fact;
    public bool staticOne = false;
    private Vector3 initiallocalPosition;
    private void Start()
    {
        initiallocalPosition = transform.localPosition;
    }
    public void SetColor(Color color, bool _staticOne)
    {
        if (_staticOne) { GetComponent<TextMeshProUGUI>().color = color; }
        else { GetComponent<TextMeshPro>().color = color; }
        staticOne = _staticOne;
    }
    void Update()
    {
        if (staticOne) {
            transform.localPosition = initiallocalPosition + new Vector3((Random.Range(-1f, 1f) * (GameManager.Instance.comboMultiplier - 1)) / fact, (Random.Range(-1f, 1f) * (GameManager.Instance.comboMultiplier - 1)) / fact, 0);
        }
        else
        {
            GetComponent<TextMeshPro>().color -= new Color(0, 0, 0, Time.deltaTime * 2);
            transform.localPosition += new Vector3((Random.Range(-1f, 1f) * (GameManager.Instance.comboMultiplier - 1)) / fact, (Random.Range(-1f, 1f) * (GameManager.Instance.comboMultiplier - 1)) / fact, 0);
        }
    }
}
