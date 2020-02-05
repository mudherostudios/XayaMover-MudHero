using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vuteka
{
    public class Callbacks
    {
        public static int chain = 2;

        public static string initialCallback(out int height, out string hashHex)
        {
            //Height is where your game was first created.
            if (chain == 0) // Mainnet
            {
                height = 961667;
                hashHex = "001514ada12a520b258b20259d1f849a6bd229c0b689d95d5de6c4430fdc1c15";
            }
            else if (chain == 1) // Testnet
            {
                height = 30000;
                hashHex = "cb767a9d1e056d793bcba3e9d6ad397df3ed87020012fa693672c784c4b8ef1f";
            }
            else // Regtestnet
            {
                height = 0;
                hashHex = "6f750b36d22f1dc3d0a6e483af45301022646dfc3b3ba2187865f5a7d6d83ab1";
            }

            return "";
        }

        public static string forwardCallback(string currentState, string blockData, string undoData, out string updatedData)
        {
            WorldState world = JsonConvert.DeserializeObject<WorldState>(currentState);
            dynamic data = JsonConvert.DeserializeObject<dynamic>(blockData);
            StructureUndoData undoCollection = new StructureUndoData();
            undoCollection.structures = new Dictionary<string, StructureUndo>();

            if (blockData.Length <= 1)
            {
                updatedData = "";
                return "";
            }

            if (world == null)
            {
                world = new WorldState();
            }

            if (world.structures == null)
            {
                world.structures = new Dictionary<string, Structure>();
            }

            foreach (var move in data["moves"])
            {
                string structureID = move["txid"].ToString();
                string moveData = move["move"].ToString();

                if (moveData != null && Utility.IsValidJSON(moveData))
                {
                    JObject unparsedMoveData = JsonConvert.DeserializeObject<JObject>(moveData);
                    Line[] lines = null;

                    if (!Utility.ParseLines(ref unparsedMoveData, ref lines))
                    {
                        continue;
                    }
                    
                    Structure structure;
                    bool isOldStructure = world.structures.ContainsKey(structureID);

                    if (isOldStructure)
                    {
                        structure = world.structures[structureID];
                    }
                    else
                    {
                        structure = new Structure(lines);
                        world.structures.Add(structureID, structure);
                    }

                    //Undo Data
                    if (!undoCollection.structures.ContainsKey(structureID))
                    {
                        StructureUndo undo = new StructureUndo();

                        if (!isOldStructure)
                            undo.isNew = true;
                        else
                            undo.isNew = false;

                        undoCollection.structures.Add(structureID, undo);
                    }
                }
            }

            updatedData = JsonConvert.SerializeObject(world);
            undoData = JsonConvert.SerializeObject(undoCollection);
            return undoData;
        }

        public static string backwardCallback(string updatedData, string blockData, string undoData)
        {
            WorldState world = JsonConvert.DeserializeObject<WorldState>(updatedData);
            StructureUndoData undoCollection = JsonConvert.DeserializeObject<StructureUndoData>(undoData);

            List<string> structuresToRemove = new List<string>();

            foreach (var sPair in world.structures)
            {
                string structureID = sPair.Key;
                Structure structure = sPair.Value;
                StructureUndo undo;

                bool undoMove = false;

                if (undoCollection.structures != null)
                {
                    undoMove = undoCollection.structures.ContainsKey(structureID);

                    //Remove if it was new.
                    if (undoMove)
                    {
                        undo = undoCollection.structures[structureID];

                        if (undo.isNew)
                        {
                            structuresToRemove.Add(structureID);
                            continue;
                        }
                    }
                }
            }

            foreach(string sToDemo in structuresToRemove)
            {
                world.structures.Remove(sToDemo);
            }

            return JsonConvert.SerializeObject(world);
        }

    }
}
