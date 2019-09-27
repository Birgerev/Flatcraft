using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateWorldMenu : MonoBehaviour
{
    public InputField nameField;
    public InputField seedField;
    public GameObject nameTakenError;
    public Button CreateButton;

    public World world;

    private bool switchingMenus = false;

    // Start is called before the first frame update
    void Start()
    {
        world = new World();
        GenerateSeedByName();
    }

    // Update is called once per frame
    void Update()
    {
        if (switchingMenus)
            return;

        world.name = nameField.text;
        world.seed = int.Parse(seedField.text);

        bool worldExists = World.worldExists(world.name);

        nameTakenError.SetActive(worldExists);
        CreateButton.interactable = !worldExists;
    }

    public void Create()
    {
        switchingMenus = true;
        world.Create();
        WorldManager.world = world;
        SceneManager.LoadScene("Loading");
    }

    public void Cancel()
    {
        SceneManager.LoadScene("SingleplayerMenu");
    }

    public void GenerateSeedByName()
    {
        System.Random rnd = new System.Random(nameField.text.GetHashCode());

        seedField.text = "" + rnd.Next(int.MinValue, int.MaxValue);
    }
}
