using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSand : Entity
{
    public Material material = Material.Sand;

    public override void Start()
    {
        base.Start();

        GetComponent<SpriteRenderer>().sprite = new ItemStack(material).getSprite();
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void Update()
    {
        base.Update();

        if (isOnGround && age > 0.2f)
        {
            if(Chunk.getBlock(Vector2Int.RoundToInt((Vector2)transform.position)) == null)
                Chunk.setBlock(Vector2Int.RoundToInt((Vector2)transform.position), material);
            else
            {
                new ItemStack(material, 1).Drop(Vector2Int.RoundToInt((Vector2)transform.position));
            }
            Destroy(gameObject);
        }
    }


    public static FallingSand Create(Vector2 pos, Material mat)
    {
        GameObject obj = MonoBehaviour.Instantiate((GameObject)Resources.Load("Objects/FallingSand"));

        obj.transform.position = new Vector3(pos.x, pos.y, 0);
        obj.GetComponent<FallingSand>().material = mat;

        return obj.GetComponent<FallingSand>();
    }
}
