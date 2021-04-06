using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mirror;
using UnityEngine;

public class SaveManager : NetworkBehaviour
{
    public static float AutosaveDuration = 2;
    public static Dictionary<Location, string> blockChanges = new Dictionary<Location, string>();

    private void Start()
    {
        if(isServer)
            StartCoroutine(SaveLoop());
    }

    private IEnumerator SaveLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(AutosaveDuration);

            //Save Block Changes
            if (blockChanges.Count > 0)
            {
                var blockChangesCopy = new Dictionary<Location, string>(blockChanges);
                blockChanges.Clear();
                var worldThread = new Thread(() => { SaveBlockChanges(blockChangesCopy); });
                worldThread.Start();

                while (worldThread.IsAlive) 
                    yield return new WaitForSeconds(0.1f);
            }

            //Save Entities
            var entities = Entity.entities;

            var entityThread = new Thread(() => { SaveEntities(entities); });
            entityThread.Start();

            while (entityThread.IsAlive) 
                yield return new WaitForSeconds(0.1f);
        }
    }

    public static void SaveEntities(List<Entity> entities)
    {
        foreach (var e in entities)
            try
            {
                e.Save();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error in saving entity: " + ex.StackTrace);
            }
    }

    public static void SaveBlockChanges(Dictionary<Location, string> changes)
    {
        //Get all changed chunks
        var changedChunks = new List<ChunkPosition>();
        foreach (var blockChange in changes)
        {
            var cPos = new ChunkPosition(blockChange.Key);
            if (!changedChunks.Contains(cPos)) 
                changedChunks.Add(cPos);
        }

        //Rewrite all block changes for chunks
        foreach (var currentEditingChunk in changedChunks)
        {
            var changesForChunk = new Dictionary<Location, string>();
            foreach (var blockChange in changes) //Add block change to list if it belongs to current chunk that we are editing
                if (new ChunkPosition(blockChange.Key).Equals(currentEditingChunk))
                    changesForChunk.Add(blockChange.Key, blockChange.Value);

            if (!currentEditingChunk.HasBeenSaved()) 
                currentEditingChunk.CreateChunkPath();

            var chunkPath = WorldManager.world.getPath() + "\\chunks\\" + currentEditingChunk.dimension + "\\" +
                            currentEditingChunk.chunkX;

            foreach (var line in File.ReadAllLines(chunkPath + "\\blocks")
            ) //Add all old block changes, removing duplicates of new block changes
            {
                Location lineLoc = new Location(int.Parse(line.Split('*')[0].Split(',')[0]),
                    int.Parse(line.Split('*')[0].Split(',')[1]));
                string lineData = line.Split('*')[1] + "*" + line.Split('*')[2];

                if (!changesForChunk.ContainsKey(lineLoc))
                    changesForChunk.Add(lineLoc, lineData);
            }

            //Empty file before writing
            File.WriteAllText(chunkPath + "\\blocks", "");

            //Write all block changes
            TextWriter c = new StreamWriter(chunkPath + "\\blocks");
            foreach (var line in changesForChunk) 
                c.WriteLine(line.Key.x + "," + line.Key.y + "*" + line.Value);

            //Close file writer
            c.Close();
        }
    }
}