using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    public enum InteractionMode { Place, Select }
    [Header("Prefabs")]
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameObject spherePrefab;
    [SerializeField] private GameObject cylinderPrefab;
    [SerializeField] private GameObject capsulePrefab;
    [SerializeField] private GameObject quadPrefab;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI modeButtonText;
    [SerializeField] private ObjectSelector selector;
    private InteractionMode currentMode = InteractionMode.Place;
    private GameObject selectedPrefab;
    private Mesh selectedMesh;
    private Material selectedMaterial;
    void Start()
    {
        SelectCube();
        UpdateModeUI();
    }
    public void SelectCube()
    {
        SetSelection(cubePrefab);
        UpdateStatus("Selected: Cube");
    }
    public void SelectSphere()
    {
        SetSelection(spherePrefab);
        UpdateStatus("Selected: Sphere");
    }
    public void SelectCylinder()
    {
        SetSelection(cylinderPrefab);
        UpdateStatus("Selected: Cylinder");
    }
    public void SelectCapsule()
    {
        SetSelection(capsulePrefab);
        UpdateStatus("Selected: Capsule/Cone");
    }
    public void SelectQuad()
    {
        SetSelection(quadPrefab);
        UpdateStatus("Selected: Quad");
    }
    private void SetSelection(GameObject prefab)
    {
        if (prefab == null) return;
        selectedPrefab = prefab;
        selectedMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        selectedMaterial = prefab.GetComponent<MeshRenderer>().sharedMaterial;
        if (currentMode == InteractionMode.Select)
        {
            GameObject active = selector.GetSelectedObject();
            if (active != null)
            {
                Mesh newMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
                Material newMat = prefab.GetComponent<MeshRenderer>().sharedMaterial;
                selector.ChangeSelectedMesh(newMesh, newMat);
            }
        }
    }
    public void ToggleMode()
    {
        currentMode = (currentMode == InteractionMode.Place)
        ? InteractionMode.Select
        : InteractionMode.Place;
        UpdateModeUI();
    }
    public InteractionMode GetCurrentMode() => currentMode;
    public GameObject GetSelectedPrefab() => selectedPrefab;
    public Mesh GetSelectedMesh() => selectedMesh;
    public Material GetSelectedMaterial() => selectedMaterial;
    private void UpdateModeUI()
    {
        string modeText = currentMode == InteractionMode.Place
        ? "Mode: Place"
        : "Mode: Select";
        if (modeButtonText != null)
            modeButtonText.text = modeText;
        UpdateStatus(modeText);
    }
    private void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        Debug.Log(message);
    }
}