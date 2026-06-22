using UnityEngine;
using UnityEngine.UI;
public class SettingsManager : MonoBehaviour
{
    public Button buttonTutorial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonTutorial.onClick.AddListener(() => GameManager.Instance.ShowTutorialCompact());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
