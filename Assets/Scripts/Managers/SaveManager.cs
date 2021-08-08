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
        if (isServer)
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
                Dictionary<Location, string> blockChangesCopy = new Dictionary<Location, string>(blockChanges);
                blockChanges.Clear();
                Thread worldThread = new Thread(() => { SaveBlockChanges(blockChangesCopy); });
                worldThread.Start();

                while (worldThread.IsAlive)
                    yield return new WaitForSeconds(0.1f);
            }

            //Save Entities
            List<Entity> entities = new List<Entity>(Entity.entities);

            Thread entityThread = new Thread(() => { SaveEntities(entities); });
            entityThread.Start();
            
            while (entityThread.IsAlive)
                yield return new WaitForSeconds(0.1f);

            //Delete no longer present entities
            foreach (Chunk chunk in WorldManager.instance.chunks.Values)
            {
                chunk.DeleteNoLongerPresentEntitiesSaves();
            }
        }
    }

    public static void SaveEntities(List<Entity> entities)
    {
        foreach (Entity e in entities)
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
        List<ChunkPosition> changedChunks = new List<ChunkPosition>();
        foreach (KeyValuePair<Location, string> blockChange in changes)
        {
            ChunkPosition cPos = new ChunkPosition(blockChange.Key);
            if (!changedChunks.Contains(cPos))
                changedChunks.Add(cPos);
        }

        //Rewrite all block changes for chunks
        foreach (ChunkPosition currentEditingChunk in changedChunks)
        {
            Dictionary<Location, string> changesForChunk = new Dictionary<Location, string>();
            foreach (KeyValuePair<Location, string> blockChange in changes
            ) //Add block change to list if it belongs to current chunk that we are editing
                if (new ChunkPosition(blockChange.Key).Equals(currentEditingChunk))
                    changesForChunk.Add(blockChange.Key, blockChange.Value);

            if (!currentEditingChunk.HasBeenSaved())
                currentEditingChunk.CreateChunkPath();

            string chunkPath = WorldManager.world.getPath() + "\\chunks\\" + currentEditingChunk.dimension + "\\" +
                               currentEditingChunk.chunkX;

            foreach (string line in File.ReadAllLines(chunkPath + "\\blocks")
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
            foreach (KeyValuePair<Location, string> line in changesForChunk)
                c.WriteLine(line.Key.x + "," + line.Key.y + "*" + line.Value);

            //Close file writer
            c.Close();
        }
    }
}