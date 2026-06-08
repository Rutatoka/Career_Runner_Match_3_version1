using UnityEngine;

public class AccessoryAdjuster : MonoBehaviour
{
    [Header("Male Settings")]
    public Vector3 maleScale = Vector3.one;
    public Vector3 malePosition = Vector3.zero;
    public Vector3 maleRotation = Vector3.zero;

    [Header("Female Settings")]
    public Vector3 femaleScale = Vector3.one;
    public Vector3 femalePosition = Vector3.zero;
    public Vector3 femaleRotation = Vector3.zero;

    private void Start()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (GameManager.Instance == null) return;

        bool isMale = GameManager.Instance.characterData.gender == 0;

        if (isMale)
        {
            transform.localScale = maleScale;
            transform.localPosition = malePosition;
            transform.localRotation = Quaternion.Euler(maleRotation);
        }
        else
        {
            transform.localScale = femaleScale;
            transform.localPosition = femalePosition;
            transform.localRotation = Quaternion.Euler(femaleRotation);
        }
    }
}