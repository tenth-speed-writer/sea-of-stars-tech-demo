using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

public class ShipwrightTestSuite { 
    [UnityTest]
    public IEnumerator TestShipFromJSON()
    {
        GameObject go = new GameObject();
        go.AddComponent<Shipwright>();
        Shipwright sw = go.GetComponent<Shipwright>();
        Shipwright.ShipPlan plan = sw.PlanFromJSON("DemoShip");
        Assert.That(plan.Rooms.Length == 10);

        yield return null;
    }
}