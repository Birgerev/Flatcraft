using System;
using System.IO;
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

    private bool _useRandomSeed = false;
    private bool _disableInput;
    
    // Update is called once per frame
    private void Update()
    {
        if (_disableInput)
            return;

        //Demo template
        world.template = WorldTemplate.Skyblock;
        
        
        worldTemplateButtonText.text = "Template: " + world.template.ToString();
        FetchNameField();
        FetchSeedField();
    }

    private void FetchSeedField()
    {
        //Assign seed
        bool isSeedFieldEmpty = (seedField.text.Length == 0);
        if (!isSeedFieldEmpty)
            world.seed = seedField.text.GetHashCode();

        _useRandomSeed = isSeedFieldEmpty;
    }

    private void FetchNameField()
    {
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
            errorText.text = "World name too long!";
            error = true;
        }
        //Check if name is valid file name
        if (nameField.text.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
        {
            errorText.text = "Unsupported character!";
            error = true;
        }
        //Error handling
        errorText.gameObject.SetActive(error);
        CreateButton.interactable = !error;
    }

    public void Create()
    {
        _disableInput = true;

        //Create random seed if seed field is empty
        if (_useRandomSeed)
            world.seed = new Random().Next();

        //Save world
        world.SaveData();
        
        //Load world
        WorldManager.world = world;
        MultiplayerManager.HostGameAsync();
        LoadingMenu.Create(LoadingMenuType.LoadWorld);
    }

    public void NextWorldTemplate()
    {
        /*Disabled for demo
        int currentTemplateIndex = (int)world.template;
        int templateAmount = Enum.GetNames(typeof(WorldTemplate)).Length;
        int nextTemplateIndex = (currentTemplateIndex + 1) % templateAmount;
        WorldTemplate nextTemplate = (WorldTemplate) nextTemplateIndex;
        
        world.template = nextTemplate;*/
    }

    public void Cancel()
    {
        Instantiate(singleplayerMenuPrefab);
        Destroy(gameObject);
    }
}