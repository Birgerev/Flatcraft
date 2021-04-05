using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour
{
    public static LoadingMenu instance;
    
    public Image loadingBar;
    public Text loadingTitle;
    public Text loadingStateText;
    
    public LoadingMenuType type;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    private void Update()
    {
        if (type == LoadingMenuType.LoadWorld)
            LoadWorldMenu();
        else if (type == LoadingMenuType.ConnectServer)
            ConnectMenu();
        else if (type == LoadingMenuType.Dimension)
            ChangeDimensionMenu();
    }

    public void SetState(string name, float progress)
    {
        loadingStateText.text = name;
        loadingBar.fillAmount = progress;
    }

    public static void Create(LoadingMenuType type)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/LoadingMenu");
        GameObject obj = Instantiate(prefab);
        obj.GetComponent<LoadingMenu>().type = type;
    }

    public void LoadWorldMenu()
    {
        loadingTitle.text = "Loading World";
            
        int steps = 7;

        if (Player.localEntity == null)
        {
            SetState("Creating Player", 1f/steps);
            return;
        }
        Chunk playerChunk = new ChunkPosition(Player.localEntity.GetComponent<Player>().Location).GetChunk();
        if (playerChunk == null)
        {
            SetState("Creating Chunks", 2f/steps);
            return;
        }
        if (!playerChunk.isGenerated)
        {
            SetState("Generating Chunks", 3f/steps);
            return;
        }
        if (!playerChunk.donePlacingGeneratedBlocks)
        {
            SetState("Placing Blocks", 4f/steps);
            return;
        }
        if (!playerChunk.donePlacingBackgroundBlocks)
        {
            SetState("Placing Background Blocks", 5f/steps);
            return;
        }
        if (!playerChunk.isLightGenerated)
        {
            SetState("Waiting For Light", 6f/steps);
            return;
        }
            
        SetState("Done!", 1);
        Destroy(gameObject, 1f);
    }

    public void ConnectMenu()
    {
        loadingTitle.text = "Joining World";
            
        int steps = 7;

        if (PlayerInstance.localPlayerInstance == null)
        {
            SetState("Connecting", 1f/steps);
            return;
        }
        if (Player.localEntity == null)
        {
            SetState("Creating Player", 2f/steps);
            return;
        }
        Chunk playerChunk = new ChunkPosition(Player.localEntity.GetComponent<Player>().Location).GetChunk();
        if (playerChunk == null)
        {
            SetState("Creating Chunks", 3f/steps);
            return;
        }
        if (!playerChunk.donePlacingGeneratedBlocks)
        {
            SetState("Placing Blocks", 4f/steps);
            return;
        }
        if (!playerChunk.donePlacingBackgroundBlocks)
        {
            SetState("Placing Background Blocks", 5f/steps);
            return;
        }
        if (!playerChunk.isLightGenerated)
        {
            SetState("Waiting For Light", 6f/steps);
            return;
        }
            
        SetState("Done!", 1);
        Destroy(gameObject, 1f);
    }

    public void ChangeDimensionMenu()
    {
        
    }
}

public enum LoadingMenuType
{
    LoadWorld,
    ConnectServer,
    Dimension
}