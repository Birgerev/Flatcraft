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

        var world = GetComponentInParent<SingleplayerMenu>().worlds[transform.GetSiblingIndex()];

        nameText.text = world.name;
        var fileSize = world.getDiskSize() / 1000;
        var fileSizeRounded = Mathf.Round(fileSize * 100f) / 100f;

        var versionName = VersionController.GetVersionName(world.versionId);

        descriptionText.text = "Survival Mode (Version: "+ versionName + ", " + fileSizeRounded + "KB)";
    }

    private void Update()
    {
        if (menuManager.selectedWorld == transform.GetSiblingIndex())
        {
            button.Select();
        }
    }

    public void Click()
    {
        GetComponentInParent<SingleplayerMenu>().selectedWorld = transform.GetSiblingIndex();

        if (Time.time - lastClickTime < 0.3f) 
            GetComponentInParent<SingleplayerMenu>().Play();

        lastClickTime = Time.time;
    }
}