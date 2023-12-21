using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class PaintingEntity : Entity
{
    private static readonly Dictionary<string, Vector2> paintingTextures = new Dictionary<string, Vector2>
    {
        {"painting_skeleton", new Vector2(4, 3)}, {"painting_fireplace", new Vector2(2, 2)}
        , {"painting_wanderer", new Vector2(1, 2)}, {"painting_graham", new Vector2(1, 2)}
        , {"painting_sunset", new Vector2(2, 1)}, {"painting_creeper", new Vector2(2, 1)}
        , {"painting_albanian", new Vector2(1, 1)}, {"painting_aztec", new Vector2(1, 1)}
    };

    [EntitySaveField(false)] [SyncVar] public string paintingId = "";

    [Server]
    public override void Spawn()
    {
        base.Spawn();

        Random r = new Random();
        ContactFilter2D filter = GetPaintingFilter();
        List<string> paintings = paintingTextures.Keys.OrderBy(item => r.Next()).ToList();

        foreach (string painting in paintings)
        {
            Vector2 dimensions = paintingTextures[painting];

            SetColliderToDimensions(dimensions);
            if (GetComponent<BoxCollider2D>().OverlapCollider(filter, new Collider2D[1]) == 0)
            {
                paintingId = painting;
                break;
            }
        }

        if (string.IsNullOrEmpty(paintingId))
            Die();
    }

    [Client]
    public override void ClientInitialize()
    {
        base.ClientInitialize();

        Sprite sprite = Resources.Load<Sprite>("Sprites/" + paintingId);
        GetRenderer().sprite = sprite;

        SetColliderToDimensions();
    }

    [Client]
    public override void Initialize()
    {
        base.Initialize();

        SetColliderToDimensions();
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        CheckOverlapDeath();
    }

    [Server]
    private void CheckOverlapDeath()
    {
        if (Time.time % 3f - Time.deltaTime <= 0)
        {
            Collider2D[] paintingColliders = new Collider2D[1];
            GetComponent<BoxCollider2D>().OverlapCollider(GetPaintingFilter(), paintingColliders);
            if (paintingColliders[0] != null)
                Die();
        }
    }

    private void SetColliderToDimensions()
    {
        Vector2 dimension = paintingTextures[paintingId];

        SetColliderToDimensions(dimension);
    }

    private void SetColliderToDimensions(Vector2 dimensions)
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector2 size = new Vector2(dimensions.x * 0.9f, dimensions.y * 0.9f);
        Vector2 center = dimensions / 2 - new Vector2(0.5f, 0.5f);

        col.size = size;
        col.offset = center;
    }

    private ContactFilter2D GetPaintingFilter()
    {
        ContactFilter2D filter = new ContactFilter2D().NoFilter();
        filter.SetLayerMask(LayerMask.GetMask("Block", "Painting"));

        return filter;
    }

    public override List<ItemStack> GetDrops()
    {
        List<ItemStack> result = new List<ItemStack>();
        result.Add(new ItemStack(Material.Painting, 1));

        return result;
    }
}