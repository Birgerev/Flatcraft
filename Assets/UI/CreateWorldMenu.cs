using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateWorldMenu : MonoBehaviour
{
    public InputField nameField;
    public InputField seedField;
    public Text errorText;
    public Button CreateButton;

    public World world;
    
    private bool switchingMenus = false;

    // Start is called before the first frame update
    void Start()
    {
        world = new World();
    }

    // Update is called once per frame
    void Update()
    {
        if (switchingMenus)
            return;

        //Filter non allowed characters
        nameField.text = System.Text.RegularExpressions.Regex.Replace(nameField.text, "[^\\w\\._]", "");

        world.name = nameField.text;
        if(seedField.text.Length > 0)
            world.seed = int.Parse(seedField.text);
        else
            world.seed = 0;
        

        bool worldExists = World.worldExists(world.name);
        bool nameEmpty = (nameField.text == "");
        bool error = (worldExists || nameEmpty);

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

        if (world.seed == 0)
        {
            world.seed = new System.Random().Next();
        }
        
        world.SaveData();
        WorldManager.world = world;
        SceneManager.LoadScene("Loading");
    }

    public void Cancel()
    {
        SceneManager.LoadScene("SingleplayerMenu");
    }
}
