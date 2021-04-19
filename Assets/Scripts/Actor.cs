using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    /// <summary>
    /// The human-formatted name of this Actor, rather than its GameObject.name.
    /// </summary>
    public string AgentName;

    /// <summary>
    /// The number of ticks until this actor is polled for another judgment call.
    /// Decided after each action by that action's cost.
    /// </summary>
    public int Cooldown = 0;

    [Serializable]
    public struct ActionDesire
    {
        public string ActionType;
        public string ActorName;
        public string TargetName;
        public PhysicalForm.BodyPartDamage Damage;
        public int X;
        public int Y;
        public int CooldownOnSuccess;

        /// <summary>
        /// Create an interaction-type ActionDesire given the names of an actor and target
        /// </summary>
        /// <param name="action_type">Must be equal to "interaction" to utilize this signature.</param>
        /// <param name="actor_name">The name of an actor GameObject. Must exist and be unique among GameObjects tagged "actor".</param>
        /// <param name="target_name">The name of the target GameObject. Must exist and be unique among GameObjects tagged "interactable".</param>
        /// <param name="cooldown">The cooldown, in number of ticks, the actor will incur once this action is complete.</param>
        public ActionDesire(string action_type, string actor_name, string target_name, int cooldown=100)
        {
            // Assert that the developer is requesting the correct action_type for this signature
            if (action_type != "interaction")
            {
                throw new ArgumentException("Type must be interaction if exactly and only actor_name and target_name are specified. Got " + action_type);
            }

            // Assert that the actor exists and is unique in its name among actors
            GameObject[] actors = GameObject.FindGameObjectsWithTag("actor");
            GameObject[] actors_matching_name = (from a in actors where a.name == actor_name select a).ToArray();
            if (actors_matching_name.Length == 0)
            {
                throw new Exception("No GameObject with tag \"actor\" found for actor_name + \"" + actor_name + "\"");
            } else if (actors_matching_name.Length > 1)
            {
                throw new Exception("More than one GameObject with tag \"actor\" found for actor_name + \"" + actor_name + "\"");
            }

            // Assert that the target exists and is unique in its name among interactables
            GameObject[] interactables = GameObject.FindGameObjectsWithTag("interactable");
            GameObject[] interactables_matching_name = (from i in interactables where i.name == target_name select i).ToArray();
            if (interactables_matching_name.Length == 0)
            {
                throw new Exception("No GameObject with tag \"interactable\" found for target_name \"" + target_name + "\"");
            } else if (interactables_matching_name.Length > 1)
            {
                throw new Exception("No GameObject with tag \"interactable\" found for target_name \"" + target_name + "\"");
            }

            this.ActionType = action_type;
            this.ActorName = actor_name;
            this.TargetName = target_name;
            this.Damage = new PhysicalForm.BodyPartDamage();
            this.X = -1;
            this.Y = -1;
            this.CooldownOnSuccess = cooldown;
        }

        /// <summary>
        /// Create an attack-type ActionDesire given an actor, a target, and a damage roll.
        /// DEVNOTE: We may need to add chance-to-hit logic here as well!
        /// </summary>
        /// <param name="action_type">Must be equal to "attack" to utilize this signature.</param>
        /// <param name="actor_name">Must exist and be unique among GameObjects tagged "actor".</param>
        /// <param name="target_name">Must exist and be unique among GameObjects tagged "actor".</param>
        /// <param name="damage">The damage to apply if this attack succeeds.</param>
        /// <param name="cooldown">The cooldown, in number of ticks, the actor will incur once this action is complete.</param>
        public ActionDesire(string action_type, string actor_name, string target_name, PhysicalForm.BodyPartDamage damage, int cooldown=10)
        {
            if (action_type != "attack")
            {
                throw new ArgumentException("Type must be attack if exactly and only actor_name, target_name, and damage are given!");
            }

            // Filter out actors
            GameObject[] actors = GameObject.FindGameObjectsWithTag("actor");

            // Assert that the assailant exists and is unique in its name amongst actors
            GameObject[] actors_matching_assailant = (from a in actors where a.name == actor_name select a).ToArray();
            if (actors_matching_assailant.Length == 0)
            {
                throw new Exception("No GameObject with tag \"actor\" found for actor_name \"" + actor_name + "\"");
            } else if (actors_matching_assailant.Length > 1)
            {
                throw new Exception("More than one GameObject with tag \"actor\" found for actor_name + \"" + actor_name + "\"");
            }

            // Assert that the victim exists and is unique in its name amongst actors
            GameObject[] actors_matching_victim = (from a in actors where a.name == target_name select a).ToArray();
            if (actors_matching_victim.Length == 0)
            {
                throw new Exception("No GameObject with tag \"actor\" found for target_name\"" + target_name + "\"");
            } else if (actors_matching_victim.Length > 1)
            {
                throw new Exception("More than one GameObject with tag \"actor\" found for target_name + \"" + target_name + "\"");
            }

            this.ActionType = action_type;
            this.ActorName = actor_name;
            this.TargetName = target_name;
            this.Damage = damage;
            this.X = -1;
            this.Y = -1;
            this.CooldownOnSuccess = cooldown;
        }

        /// <summary>
        /// Create a movement-type ActionDesire given the name of an actor and x & y positions to move to.
        /// </summary>
        /// <param name="action_type">Must be equal to "movement" in order to utilize this signature.</param>
        /// <param name="actor_name">Must exist and be unique among GameObjects tagged "actor".</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cooldown">The cooldown, in number of ticks, the actor will incur once this action is complete.</param>
        public ActionDesire(string action_type, string actor_name, int x, int y, int cooldown=10)
        {
            if (action_type != "movement")
            {
                throw new ArgumentException("Type must be movement if exactly and only actor_name, x, and y are given!");
            }

            // Assert that the mover exists and is unique in its name amongst actors
            GameObject[] actors = GameObject.FindGameObjectsWithTag("actor");
            GameObject[] actors_matching_mover = (from a in actors where a.name == actor_name select a).ToArray();
            if (actors_matching_mover.Length == 0)
            {
                throw new Exception("No GameObject with the tag \"actor\" found for actor_name \"" + actor_name + "\"");
            } else if (actors_matching_mover.Length > 1)
            {
                throw new Exception("More than one GameObject with tag \"actor\" found for actor_name \"" + actor_name + "\"");
            }

            this.ActionType = action_type;
            this.ActorName = actor_name;
            this.TargetName = "";
            this.Damage = new PhysicalForm.BodyPartDamage();
            this.X = x;
            this.Y = y;
            this.CooldownOnSuccess = cooldown;
        }
    }

    /// <summary>
    /// An overridable method called at each tick when the cooldown of this Actor is equal to zero.
    /// </summary>
    public virtual void Act()
    {

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // At each tick, decrement the cooldown (if appropriate) and act (if cooldown is then zero).
        if (Cooldown != 0)
        {
            Cooldown -= 1;
        }

        if (Cooldown == 0)
        {
            Act();
        }
    }
}
