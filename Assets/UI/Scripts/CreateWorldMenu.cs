using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class CreateWorldMenu : MonoBehaviour
{
    public Button CreateButton;
    public Text errorText;
    public InputField nameField;
    public InputField seedField;

    private bool switchingMenus;

    public World world;

    // Start is called before the first frame update
    private void Start()
    {
        world = new World();
    }

    // Update is called once per frame
    private void Update()
    {
        if (switchingMenus)
            return;

        //Filter non allowed characters
        nameField.text = Regex.Replace(nameField.text, "[^\\w\\._]", "");

        world.name = nameField.text;
        if (seedField.text.Length > 0)
            world.seed = int.Parse(seedField.text);
        else
            world.seed = 0;


        var worldExists = World.worldExists(world.name);
        var nameEmpty = nameField.text == "";
        var error = worldExists || nameEmpty;

        if (worldExists)
            errorText.text = "World name is already taken!";
        if (nameEmpty)
            errorText.text = "No world name!";

        errorText.gameObject.SetActive(error);
        CreateButton.interactable = !error;
    }

    public void Create()
    {
        switchingMenus = true;

        if (world.seed == 0) world.seed = new Random().Next();
        world.versionId = VersionController.CurrentVersionId;

        world.SaveData();
        WorldManager.world = world;
        SceneManager.LoadScene("Game");
        GameNetworkManager.isHost = true;
        LoadingMenu.Create(LoadingMenuType.LoadWorld);
    }

    public void Cancel()
    {
        SceneManager.LoadScene("SingleplayerMenu");
    }
}