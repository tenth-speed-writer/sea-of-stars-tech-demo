using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PhysicalForm : MonoBehaviour
{
    // The percentage of total integrity remaining in a body part layer
    // at the start of a damage roll past which damage of each type
    // will attempt to spill over into the next layer underneath.
    public float ImpactSpilloverPoint = 0.30f;
    public float ShearSpilloverPoint = 0.45f;
    public float CorrosiveSpilloverPoint = 0.80f;
    public float EnergySpilloverPoint = 0.65f;

    public Limb[] Limbs;
    public LimbJoint[] Joints;

    [Serializable]
    public struct Substance
    {
        /// <summary>
        /// The name of this substance.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The density of this substance in kg/m3 or g/L. (They're equivalent.)
        /// For perspective, water (which is close to flesh) is about 1000 g/L.
        /// </summary>
        public readonly float Density;

        /// <summary>
        /// How much integrity is bestowed to a BodyPart per liter of this substance that composes it.
        /// </summary>
        public readonly float IntegrityPerLiter;

        /// <summary>
        /// Impact resistance factor of this substance, on a scale of [-1.0, 1.0].
        /// </summary>
        public readonly float ImpactResist;

        /// <summary>
        /// Shear resistance factor of this substance, on a scale of [-1.0, 1.0].
        /// </summary>
        public readonly float ShearResist;

        /// <summary>
        /// Corrosion resistance factor of this substance, on a scale of [-1.0, 1.0].
        /// </summary>
        public readonly float CorrosiveResist;

        /// <summary>
        /// Energy resistance factor of this substance, on a scale of [-1.0, 1.0].
        /// </summary>
        public readonly float EnergyResist;

        /// <summary>
        /// A struct representing a physical substance in the Sea of Stars setting.
        /// This can be anything from flesh and bone to materia and copper wiring.
        /// </summary>
        /// <param name="name">A unique name for the substance in question</param>
        /// <param name="density">The density of this material, in kg/m3 or g/L.</param>
        /// <param name="integrity_per_liter">Amount of integrity imparted to a BodyPart per liter of this substance in its composition.</param>
        /// <param name="impact_resist">A float in [-1, 1], to be averaged in determing the resulting BodyPart's resistances.</param>
        /// <param name="shear_resist">A float in [-1, 1], to be averaged in determing the resulting BodyPart's resistances.</param>
        /// <param name="corrosive_resist">A float in [-1, 1], to be averaged in determing the resulting BodyPart's resistances.</param>
        /// <param name="energy_resist">A float in [-1, 1], to be averaged in determing the resulting BodyPart's resistances.</param>
        public Substance(string name,
                         float density,
                         float integrity_per_liter,
                         float impact_resist,
                         float shear_resist,
                         float corrosive_resist,
                         float energy_resist)
        {
            // Validate arguments
            if (name.Length == 0)
            {
                throw new ArgumentException("Argument 'name' must not be an empty string.");
            }
            else if (density <= 0)
            {
                throw new ArgumentException(String.Format("Argument 'density' must be greater than zero kg/m3, got {0}"),
                                            density.ToString());
            }
            else if (integrity_per_liter <= 0)
            {
                throw new ArgumentException(String.Format("Argument 'integrity_per_liter' must be greater than zero; got {0}",
                                                           integrity_per_liter.ToString()));
            }
            else if (impact_resist < -1.0f || impact_resist > 1.0f)
            {
                throw new ArgumentException(String.Format("impact_resist must be in range [-1.0, 1.0]; got {0}"),
                                            impact_resist.ToString());
            }
            else if (shear_resist < -1.0f || shear_resist > 1.0f)
            {
                throw new ArgumentException(String.Format("shear_resist must be in range [-1.0, 1.0]; got {0}"),
                                            shear_resist.ToString());
            }
            else if (corrosive_resist < -1.0f || impact_resist > 1.0f)
            {
                throw new ArgumentException(String.Format("corrosive_resist must be in range [-1.0, 1.0]; got {0}"),
                                            corrosive_resist.ToString());
            }
            else if (energy_resist < -1.0f || energy_resist > 1.0f)
            {
                throw new ArgumentException(String.Format("energy_resist must be in range [-1.0, 1.0]; got {0}"),
                                            energy_resist.ToString());
            }

            this.Name = name;
            this.Density = density;
            this.IntegrityPerLiter = integrity_per_liter;
            this.ImpactResist = impact_resist;
            this.ShearResist = shear_resist;
            this.CorrosiveResist = corrosive_resist;
            this.EnergyResist = energy_resist;
        }
    }

    /// <summary>
    /// A utility struct representing a named pair of a Substance and a Volume there-of
    /// </summary>
    [Serializable]
    public struct SubstanceAndVolume
    {
        public Substance Substance;
        public float Volume;

        public SubstanceAndVolume(Substance substance, float volume)
        {
            // Assert that volume is positive non-zero
            if (volume <= 0)
            {
                throw new ArgumentException(String.Format("Argument 'volume' must be greater than zero. Given {0}"),
                                                          volume.ToString());
            }

            this.Substance = substance;
            this.Volume = volume;
        }
    }

    /// <summary>
    /// A struct representing a single part of a larger body, including its physical stats and integrity.
    /// </summary>
    [Serializable]
    public struct BodyPart
    {
        public string Name;
        public string[] Substances;
        public float Volume;
        public float Mass;
        public float MaxIntegrity;
        public float Integrity;
        public float ImpactResist;
        public float ShearResist;
        public float CorrosiveResist;
        public float EnergyResist;

        public BodyPart(string name, SubstanceAndVolume[] components)
        {
            this.Name = name;
            this.Substances = (from pair in components select pair.Substance.Name).ToArray();

            // Calculate material-derived stats
            // Divide mass by 1000 since volume is in L instead of m3
            this.Mass = (float)(from pair in components select pair.Substance.Density * pair.Volume / 1000).Sum();
            this.Volume = (float)(from pair in components select pair.Volume).Sum();
            this.MaxIntegrity = (float)(from pair in components select pair.Substance.IntegrityPerLiter * pair.Volume).Sum();
            this.Integrity = MaxIntegrity;

            // Calculate resistances as volume-weighted averages
            this.ImpactResist = (float)(from pair in components
                                        select pair.Substance.ImpactResist * pair.Volume).Sum() / Volume;
            this.ShearResist = (float)(from pair in components
                                       select pair.Substance.ShearResist * pair.Volume).Sum() / Volume;
            this.CorrosiveResist = (float)(from pair in components
                                           select pair.Substance.CorrosiveResist * pair.Volume).Sum() / Volume;
            this.EnergyResist = (float)(from pair in components
                                        select pair.Substance.EnergyResist * pair.Volume).Sum() / Volume;
        }
    }

    /// <summary>
    /// A struct representing a single limb with multiple layers, each having one or more BodyParts.
    /// Damage penetration and overflow is applied from the last layer inward to random constituents.
    /// 
    /// DEVNOTE: Be aware that the Name attribute MUST be unique between Limbs of a single PhysicalForm.
    /// </summary>
    [Serializable]
    public struct Limb
    {
        public string Name;
        public BodyPart[][] Layers;

        public Limb(string name, BodyPart[][] layers)
        {
            // Assertion is commented out for now because we -will- need limbs to be able
            // to represent themselves as in a destroyed state for a single frame's time.

            //// Assert layers--and especially its first element--are not empty.
            //if (layers.Length == 0)
            //{
            //    throw new ArgumentException("At least one layer must be defined!");
            //} else if (layers[0].Length == 0)
            //{
            //    throw new ArgumentException("The first (innermost) layer of a Limb cannot be empty!");
            //}

            //// Make sure that there are no empty layers in between occupied layers.
            //for (int i = 1; i < layers.Length; i++)
            //{
            //    if (layers[i].Length != 0 & layers[i-1].Length == 0) {
            //        throw new ArgumentException("Cannot have an empty layer beneath (at a lower index than) an occupied layer!");
            //    }
            //}

            this.Name = name;
            this.Layers = layers;
        }
    }

    /// <summary>
    /// A struct representing quantities of the four possible damage types of an attack.
    /// </summary>
    [Serializable]
    public struct BodyPartDamage
    {
        public float Impact;
        public float Shear;
        public float Energy;
        public float Corrosive;

        public BodyPartDamage(float impact = 0, float shear = 0, float energy = 0, float corrosive = 0)
        {
            // Validate inputs. They may be equal to but not less than zero.
            if (impact < 0)
            {
                throw new ArgumentException(String.Format("'impact' must be no less than zero; got {0}"), impact.ToString());
            }
            else if (shear < 0)
            {
                throw new ArgumentException(String.Format("'shear' must be no less than zero; got {0}"), shear.ToString());
            }
            else if (energy < 0)
            {
                throw new ArgumentException(String.Format("'energy' must be no less than zero; got {0}"), energy.ToString());
            }
            else if (corrosive < 0)
            {
                throw new ArgumentException(String.Format("'corrosive' must be no less than zero; got {0}"), corrosive.ToString());
            }

            this.Impact = impact;
            this.Shear = shear;
            this.Energy = energy;
            this.Corrosive = corrosive;
        }
    }

    /// <summary>
    /// Represents a single pair of limbs connected by a joint of some kind.
    /// 
    /// If the limb with the name origin_name is destroyed, so will the limb
    /// with the name extension_name. This in turn can trickle down recursively.
    /// </summary>
    [Serializable]
    public struct LimbJoint
    {
        public string origin_name;
        public string extension_name;

        public LimbJoint(string origin_name, string dependent_name)
        {
            this.origin_name = origin_name;
            this.extension_name = dependent_name;
        }
    }

    /// <summary>
    /// Returns a BodyPart instance with damage applied to it. Draws a floor at 0 integrity.
    /// </summary>
    /// <param name="damage">A struct with values of impact, shear, corrosive, and energy damage.</param>
    /// <param name="part">A BodyPart to be transmuted and returned</param>
    /// <returns>A transmuted BodyPart with the given damage applied.</returns>
    public BodyPart DamagePart(BodyPartDamage damage, BodyPart part)
    {
        // Since part is a struct, it'll be passed to this method by value instead of reference. Thus, we can mutate it freely and return it.
        // A resistance of 1 will negate damage, and a resistance of -1 will double it.
        part.Integrity = Math.Max(0f, part.Integrity - damage.Impact * (1 - part.ImpactResist));
        part.Integrity = Math.Max(0f, part.Integrity - damage.Shear * (1 - part.ShearResist));
        part.Integrity = Math.Max(0f, part.Integrity - damage.Corrosive * (1 - part.CorrosiveResist));
        part.Integrity = Math.Max(0f, part.Integrity - damage.Energy * (1 - part.EnergyResist));

        return part;
    }

    /// <summary>
    /// Returns a limb with the specified damage applied to it, including layer penetration/spillover.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="limb"></param>
    /// <returns></returns>
    public Limb DamageLimb(BodyPartDamage damage, Limb limb)
    {
        // Pick a hypothetical full-penetration path, rolling weighted draws based on volume at each layer
        BodyPart[] targets = (from layer in limb.Layers
                              select (BodyPart)Util.WeightedRandomDraw(choices: layer,
                                                                       weights: (from p in layer select p.Volume).ToArray())).ToArray();

        // Record targets' current integrities as an initial-state reference
        float[] initial_integrities = (from tgt in targets select tgt.Integrity).ToArray();

        // Create a tally of remaining damage to be distributed
        BodyPartDamage dmg_left = damage;

        // Iterate through targets, going outside in and breaking early if we run out of damage to distribute.
        for (int i = targets.Length - 1; i >= 0; i--)
        {
            // Identify the index of this part in its own layer array
            int part_index = Array.IndexOf(limb.Layers[i], targets[i]);

            // Apply impact damage, in whole or in part
            if (dmg_left.Impact / initial_integrities[i] > ImpactSpilloverPoint)
            {
                // With spillover
                float impact_amt = dmg_left.Impact * ImpactSpilloverPoint;
                limb.Layers[i][part_index] = DamagePart(damage: new BodyPartDamage(impact: impact_amt),
                                                        part: targets[i]);
                dmg_left.Impact -= impact_amt;
            } else
            {
                // Without spillover
                limb.Layers[i][part_index] = DamagePart(damage: new BodyPartDamage(impact: dmg_left.Impact),
                                                        part: limb.Layers[i][part_index]);

                dmg_left.Impact = 0f;
            }

            // Apply shear damage, in whole or part
            if (dmg_left.Shear / initial_integrities[i] > ShearSpilloverPoint)
            {
                // With spillover
                float shear_amt = dmg_left.Shear * ShearSpilloverPoint;
                limb.Layers[i][part_index] = DamagePart(damage: new BodyPartDamage(shear: shear_amt),
                                                        part: limb.Layers[i][part_index]);

                dmg_left.Shear -= shear_amt;
            } else
            {
                // Without spillover
                limb.Layers[i][part_index] = DamagePart(damage: new BodyPartDamage(shear: dmg_left.Shear),
                                                        part: limb.Layers[i][part_index]);

                dmg_left.Shear = 0f;
            }

            // Apply corrosive damage, in whole or part
            if (dmg_left.Corrosive / initial_integrities[i] > CorrosiveSpilloverPoint)
            {
                // With spillover
                float corrosive_amt = dmg_left.Corrosive * CorrosiveSpilloverPoint;
                limb.Layers[i][part_index] = DamagePart(damage: new BodyPartDamage(corrosive: corrosive_amt),
                                                        part: limb.Layers[i][part_index]);

                dmg_left.Corrosive -= corrosive_amt;
            } else
            {
                // Without spillover
                limb.Layers[i][part_index] = DamagePart(damage: new BodyPartDamage(corrosive: dmg_left.Corrosive),
                                                        part: limb.Layers[i][part_index]);

                dmg_left.Corrosive = 0f;
            }

            // Apply energy damage, in whole or part
            if (dmg_left.Energy / initial_integrities[i] > EnergySpilloverPoint)
            {
                // With spillover
                float energy_amt = dmg_left.Energy * EnergySpilloverPoint;
                limb.Layers[i][part_index] = DamagePart(damage: new BodyPartDamage(energy: energy_amt),
                                                        part: limb.Layers[i][part_index]);

                dmg_left.Energy -= energy_amt;
            } else
            {
                // Without spillover
                limb.Layers[i][part_index] = DamagePart(damage: new BodyPartDamage(energy: dmg_left.Energy),
                                                        part: limb.Layers[i][part_index]);

                dmg_left.Energy = 0f;
            }

            // Lastly, if no further damage is left to spill over, break the loop.
            // Otherwise, we'll carry that damage forward one layer deeper.
            if (dmg_left.Impact + dmg_left.Shear + dmg_left.Corrosive + dmg_left.Energy == 0)
            {
                break;
            }
        }

        // If any damage remains after all penetration has been accounted for,
        // then divide it evenly among parts which were damaged by this attack.
        if (dmg_left.Impact + dmg_left.Shear + dmg_left.Corrosive + dmg_left.Energy != 0)
        {
            for (int i = targets.Length - 1; i >= 0; i--)
            {
                int part_index = Array.IndexOf(limb.Layers[i], targets[i]);
                limb.Layers[i][part_index] = DamagePart(damage: new BodyPartDamage(impact: dmg_left.Impact / targets.Length,
                                                                                   shear: dmg_left.Shear / targets.Length,
                                                                                   corrosive: dmg_left.Corrosive / targets.Length,
                                                                                   energy: dmg_left.Energy / targets.Length),
                                                        part: limb.Layers[i][part_index]);
            }
        }

        // Finally, return the transmuted Limb.
        return limb;
    }

    public void DestroyLimb(string limb_name)
    {
        // Determine which limbs are attached through the one being destroyed
        string[] dependents_names = (string[])(from j in Joints
                                               where j.origin_name == limb_name
                                               select j.extension_name);

        // Destroy them recursively
        foreach (string n in dependents_names)
        {
            DestroyLimb(limb_name: n);
        }

        // Finally, destroy this limb by removing it and its joints from their respective arrays
        Limbs = (Limb[])(from limb in Limbs
                         where limb.Name != limb_name
                         select limb);
        Joints = (LimbJoint[])(from joint in Joints
                               where joint.origin_name != limb_name
                               select joint);
    }

    public void TakeDamage(BodyPartDamage damage)
    {
        // Select a limb to take damage, weighing the draw by total volume.
        // Start by getting an ordered list of the total volume of each limb.
        float[] limb_volumes = (from limb in Limbs
                                select (from layer in limb.Layers
                                        select (from part in layer
                                                select part.Volume).Sum()).Sum()).ToArray();
        Limb target = (Limb)Util.WeightedRandomDraw(Limbs, limb_volumes);

        // Apply DamageLimb and update that limb
        Limb damaged_tgt = DamageLimb(damage: damage,
                                      limb: target);
        int tgt_index = Array.IndexOf(Limbs, target);
        Limbs[tgt_index] = damaged_tgt;

        // If that limb's innermost layer is totally destroyed, destroy it and any limbs linked through it.
        // DEVNOTE: This approach REQUIRES that limbs be uniquely named.
        BodyPart[] tgt_innermost_layer = damaged_tgt.Layers[0];
        bool target_inner_layer_destroyed = (from part in tgt_innermost_layer select part.Integrity).Sum() == 0;
        if (target_inner_layer_destroyed)
        {
            DestroyLimb(damaged_tgt.Name);
        }

        // TODO: See if this entity should die as a result of the damage.
        
        // TODO: If necessary, call whatever method handles death.
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Tick any organs that do things over time
    }
}
