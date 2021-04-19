using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shipwright : MonoBehaviour
{
    /// <summary>
    /// A struct representing a room--including its name and the cells which comprise it.
    /// </summary>
    [System.Serializable]
    public struct Room {
        public string Name;
        public Util.Coord2D[] Cells;

        public Room(Util.Coord2D[] cells,
                    string name = "New Room")
        {
            // Assert argument 'cells' isn't an empty array
            if (cells.Length == 0)
            {
                throw new ArgumentException("A room must consist of at least one cell!");
            }
            Name = name;
            Cells = cells;
        }
    }

    /// <summary>
    /// A struct representing the layout of a ship as a 2D array filled with arrays of strings
    /// representing entities to spawn there, as well as definitions of rooms there-within.
    /// </summary>
    [System.Serializable]
    public struct ShipPlan
    {
        public string[][][] Cells;
        public Room[] Rooms;

        public ShipPlan(string[][][] cells, Room[] rooms)
        {
            Cells = cells;
            Rooms = rooms;
        }

        public ShipPlan(string[][][] cells)
        {
            Cells = cells;
            Rooms = new Room[] { };
        }
    }

    public ShipPlan PlanFromJSON(string json_name)
    {
        //var foo = Util.ImportJson<ShipPlan[]>("json/" + json_name);
        //return foo[0];

        TextAsset contents = Resources.Load<TextAsset>("json/" + json_name);
        return JsonUtility.FromJson<ShipPlan>(contents.text);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
;