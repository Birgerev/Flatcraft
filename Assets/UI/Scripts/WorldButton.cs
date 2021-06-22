using UnityEngine;
using UnityEngine.UI;

public class WorldButton : MonoBehaviour
{
    public Text descriptionText;

    public float lastClickTime;
    public Text nameText;
    public Button button;
    private SingleplayerMenu menuManager;

    private void Start()
    {
        menuManager = GetComponentInParent<SingleplayerMenu>();

        World world = GetComponentInParent<SingleplayerMenu>().worlds[transform.GetSiblingIndex()];

        nameText.text = world.name;
        float fileSize = world.getDiskSize() / 1000;
        float fileSizeRounded = Mathf.Round(fileSize * 100f) / 100f;

        string versionName = VersionController.GetVersionName(world.versionId);

        descriptionText.text = "Survival Mode (Version: " + versionName + ", " + fileSizeRounded + "KB)";
    }

    private void Update()
    {
        if (menuManager.selectedWorld == transform.GetSiblingIndex())
            button.Select();
    }

    public void Click()
    {
        GetComponentInParent<SingleplayerMenu>().selectedWorld = transform.GetSiblingIndex();

        if (Time.time - lastClickTime < 0.3f)
            GetComponentInParent<SingleplayerMenu>().Play();

        lastClickTime = Time.time;
    }
}