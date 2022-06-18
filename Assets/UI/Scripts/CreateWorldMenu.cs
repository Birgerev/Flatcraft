using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class CreateWorldMenu : MonoBehaviour
{
    public GameObject singleplayerMenuPrefab;
    public Button CreateButton;
    public Text errorText;
    public Text worldTemplateButtonText;
    public InputField nameField;
    public InputField seedField;

    public World world = new World();

    private bool randomSeed = false;
    private bool disableInput;
    
    // Update is called once per frame
    private void Update()
    {
        if (disableInput)
            return;

        worldTemplateButtonText.text = "Template: " + world.template.ToString();
        FetchNameField();
        FetchSeedField();
    }

    private void FetchSeedField()
    {
        //Assign seed
        bool seedFieldEmpty = (seedField.text.Length == 0);
        if (!seedFieldEmpty)
            world.seed = int.Parse(seedField.text);
        randomSeed = seedFieldEmpty;
    }

    private void FetchNameField()
    {
        //Filter non allowed characters
        nameField.text = Regex.Replace(nameField.text, "[^\\w\\._]", "");
        
        //Apply world name
        world.name = nameField.text;
        
        bool error = false;
        //Check if world name is taken
        if (World.WorldExists(world.name))
        {
            errorText.text = "World name is already taken!";
            error = true;
        }
        //Check if name field is empty
        if (nameField.text.Length == 0)
        {
            errorText.text = "No world name!";
            error = true;
        }
        //Check if name is too long
        if (nameField.text.Length > 30)
        {
            errorText.text = "World too long!";
            error = true;
        }
        //Error handling
        errorText.gameObject.SetActive(error);
        CreateButton.interactable = !error;
    }

    public void Create()
    {
        disableInput = true;

        //Create random seed if seed field is empty
        if (randomSeed)
            world.seed = new Random().Next();

        //Save world
        world.SaveData();
        
        //Load world
        WorldManager.world = world;
        GameNetworkManager.connectionMode = ConnectionMode.Host;
        GameNetworkManager.StartGame();
        LoadingMenu.Create(LoadingMenuType.LoadWorld);
    }

    public void NextWorldTemplate()
    {
        int currentTemplateIndex = (int)world.template;
        int templateAmount = Enum.GetNames(typeof(WorldTemplate)).Length;
        int nextTemplateIndex = (currentTemplateIndex + 1) % templateAmount;
        WorldTemplate nextTemplate = (WorldTemplate) nextTemplateIndex;
        
        world.template = nextTemplate;
    }

    public void Cancel()
    {
        Instantiate(singleplayerMenuPrefab);
        Destroy(gameObject);
    }
}