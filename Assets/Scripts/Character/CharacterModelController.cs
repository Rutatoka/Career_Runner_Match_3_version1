using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterModelController : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    public string idleStateName = "Idle";
    public string runStateName = "Run";

    [Header("Gender Models")]
    public Transform bodyContainer;
    public GameObject maleModelPrefab;
    public GameObject femaleModelPrefab;
    private Material torsoMaterial;
    private Material pantsMaterial;
    private Material shoesMaterial;

    [Header("Manual Materials")]
    public Material femaleTorsoMaterial;
    public Material femalePantsMaterial;
    public Material femaleShoesMaterial;

    [Header("Bone Names")]
    public string maleSpineBoneName = "mixamorig:Spine";
    public string maleHeadBoneName = "mixamorig:Head";
    public string femaleSpineBoneName = "mixamorig1:Spine";
    public string femaleHeadBoneName = "mixamorig1:Head";

    [Header("Name Display")]
    public TMP_Text nameText;
    public Vector3 nameOffset = new Vector3(0, 2.5f, 0);

    [Header("Settings")]
    public bool isInGame = false;

    [Header("Rotation Container")]
    public Transform modelContainer;

    private GameObject currentModel;
    private bool isMale;

    private void Start()
    {
        if (GameManager.Instance != null && !isInGame)
        {
            if (GameManager.Instance.State == GameManager.GameState.Game)
            {
                isInGame = true;
            }
        }
        UpdateCharacter();
    }

    private void LateUpdate()
    {
        if (currentModel != null && isInGame)
        {
            currentModel.transform.localPosition = Vector3.zero;
        }
    }

    public void UpdateCharacter()
    {
        if (GameManager.Instance == null) return;

        if (nameText != null)
        {
            nameText.text = GameManager.Instance.characterData.characterName;
            nameText.transform.localPosition = nameOffset;
        }

        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
        }

        isMale = GameManager.Instance.characterData.gender == 0;
        GameObject prefab = isMale ? maleModelPrefab : femaleModelPrefab;
        if (prefab != null)
        {
            currentModel = Instantiate(prefab, modelContainer);
            currentModel.transform.localPosition = Vector3.zero;

            animator = currentModel.GetComponent<Animator>();
            if (animator == null)
            {
                animator = currentModel.AddComponent<Animator>();
            }

            if (animator != null)
            {
                if (isInGame)
                {
                    animator.SetFloat("Speed", 1f);
                    animator.Play(runStateName, 0, 0f);
                }
                else
                {
                    animator.SetFloat("Speed", 0f);
                    animator.Play(idleStateName, 0, 0f);
                }
            }

            FindAndCacheMaterials();
            ApplyClothColors();
        }

        if (modelContainer != null)
        {
            modelContainer.localRotation = isInGame ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            modelContainer.localScale = Vector3.one;
        }

        string spineName = isMale ? maleSpineBoneName : femaleSpineBoneName;
        string headName = isMale ? maleHeadBoneName : femaleHeadBoneName;

        Transform spine = FindBone(spineName);
        Transform head = FindBone(headName);

        ClearAccessories(spine);
        ClearAccessories(head);

        if (!string.IsNullOrEmpty(GameManager.Instance.characterData.equippedAccessory))
        {
            GameObject accessory = Resources.Load<GameObject>(GameManager.Instance.characterData.equippedAccessory);
            if (accessory != null && head != null)
            {
                InstantiateAccessory(accessory, head);
            }
        }

        if (!string.IsNullOrEmpty(GameManager.Instance.characterData.equippedAppearance))
        {
            GameObject appearance = Resources.Load<GameObject>(GameManager.Instance.characterData.equippedAppearance);
            if (appearance != null && head != null)
            {
                InstantiateAccessory(appearance, head);
            }
        }
    }

    public void UpdateClothColors()
    {
        if (currentModel == null)
        {
            UpdateCharacter();
            return;
        }

        FindAndCacheMaterials();
        ApplyClothColors();
    }

    private void FindAndCacheMaterials()
    {
        if (currentModel == null) return;

        SkinnedMeshRenderer[] renderers = currentModel.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        if (!isMale && femaleTorsoMaterial != null)
        {
            torsoMaterial = femaleTorsoMaterial;
            pantsMaterial = femalePantsMaterial;
            shoesMaterial = femaleShoesMaterial;
            return;
        }

        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                string matName = mat.name.ToLower();
                if (matName.Contains("tops") || matName.Contains("shirt") ||
                    matName.Contains("body") || matName.Contains("torso"))
                {
                    torsoMaterial = mat;
                }
                else if (matName.Contains("bottoms") || matName.Contains("pants") ||
                         matName.Contains("legs"))
                {
                    pantsMaterial = mat;
                }
                else if (matName.Contains("shoes") || matName.Contains("boots") ||
                         matName.Contains("foot"))
                {
                    shoesMaterial = mat;
                }
            }
        }

        if (torsoMaterial == null && renderers.Length > 0 && renderers[0].materials.Length > 0)
        {
            torsoMaterial = renderers[0].materials[0];
        }
    }

    private void ApplyClothColors()
    {
        if (GameManager.Instance == null) return;
        CharacterData data = GameManager.Instance.characterData;

        if (torsoMaterial != null) torsoMaterial.color = data.torsoColor;
        if (pantsMaterial != null) pantsMaterial.color = data.pantsColor;
        if (shoesMaterial != null) shoesMaterial.color = data.shoesColor;
    }

    private Transform FindBone(string boneName)
    {
        if (currentModel == null) return null;

        Transform[] bones = currentModel.GetComponentsInChildren<Transform>(true);
        foreach (Transform bone in bones)
        {
            if (bone.name == boneName) return bone;
        }
        return null;
    }

    private void ClearAccessories(Transform bone)
    {
        if (bone == null) return;

        List<Transform> toRemove = new List<Transform>();
        foreach (Transform child in bone)
        {
            if (child.CompareTag("Accessory") || child.CompareTag("Appearance"))
            {
                toRemove.Add(child);
            }
        }

        foreach (Transform item in toRemove)
        {
            Destroy(item.gameObject);
        }
    }

    private GameObject InstantiateAccessory(GameObject prefab, Transform parentBone)
    {
        if (prefab == null || parentBone == null) return null;

        GameObject accessoryInstance = Instantiate(prefab, parentBone);
        AccessoryAdjuster adjuster = accessoryInstance.GetComponent<AccessoryAdjuster>();
        if (adjuster != null) adjuster.ApplySettings();
        return accessoryInstance;
    }

    public void SetInGame(bool value)
    {
        isInGame = value;
        UpdateCharacter();
    }
}
