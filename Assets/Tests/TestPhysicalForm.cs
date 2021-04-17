using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

public class PhysicalFormTestSuite
{
    public GameObject billy_bob;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // At each iteration, create a new GameObject with its own new PhysicalForm
        billy_bob = new GameObject();
        billy_bob.AddComponent<PhysicalForm>();
        PhysicalForm body = (PhysicalForm)billy_bob.GetComponent("PhysicalForm");
        
        // Create a couple of mock substances
        PhysicalForm.Substance flesh = new PhysicalForm.Substance(name: "flesh",
                                                                  density: 1010f,
                                                                  integrity_per_liter: 10f,
                                                                  impact_resist: 0.20f,
                                                                  shear_resist: 0.0f,
                                                                  corrosive_resist: -0.15f,
                                                                  energy_resist: 0.15f);
        PhysicalForm.Substance bone = new PhysicalForm.Substance(name: "bone",
                                                                 density: 1400f,
                                                                 integrity_per_liter: 15f,
                                                                 impact_resist: -0.15f,
                                                                 shear_resist: 0.25f,
                                                                 corrosive_resist: 0.0f,
                                                                 energy_resist: 0.25f);

        // Use them to create some mock body parts
        PhysicalForm.SubstanceAndVolume[] heart_composition;
        heart_composition = new PhysicalForm.SubstanceAndVolume[] { new PhysicalForm.SubstanceAndVolume(substance: flesh, volume: 1) };
        PhysicalForm.SubstanceAndVolume[] torso_tissue_composition;
        torso_tissue_composition = new PhysicalForm.SubstanceAndVolume[] { new PhysicalForm.SubstanceAndVolume(substance: flesh, volume: 12),
                                                                           new PhysicalForm.SubstanceAndVolume(substance: bone, volume: 8) };
        PhysicalForm.SubstanceAndVolume[] spine_composition;
        spine_composition = new PhysicalForm.SubstanceAndVolume[] { new PhysicalForm.SubstanceAndVolume(substance: flesh, volume: 1),
                                                                    new PhysicalForm.SubstanceAndVolume(substance: bone, volume: 4) };
        PhysicalForm.SubstanceAndVolume[] pelvic_bone_composition;
        pelvic_bone_composition = new PhysicalForm.SubstanceAndVolume[] { new PhysicalForm.SubstanceAndVolume(substance: bone, volume: 4) };

        PhysicalForm.SubstanceAndVolume[] pelvic_tissue_composition;
        pelvic_tissue_composition = new PhysicalForm.SubstanceAndVolume[] { new PhysicalForm.SubstanceAndVolume(substance: flesh, volume: 8),
                                                                new PhysicalForm.SubstanceAndVolume(substance: bone, volume: 1) };
        PhysicalForm.SubstanceAndVolume[] geldables_composition;
        geldables_composition = new PhysicalForm.SubstanceAndVolume[] { new PhysicalForm.SubstanceAndVolume(substance: flesh, volume: 0.5f) };

        PhysicalForm.BodyPart spine = new PhysicalForm.BodyPart(name: "Spine",
                                                                components: spine_composition);
        PhysicalForm.BodyPart heart = new PhysicalForm.BodyPart(name: "Heart",
                                                                components: heart_composition);
        PhysicalForm.BodyPart torso_tissue = new PhysicalForm.BodyPart(name: "Torso Tissue",
                                                                       components: torso_tissue_composition);
        PhysicalForm.BodyPart pelvic_bone = new PhysicalForm.BodyPart(name: "Pelvic Bone",
                                                                      components: pelvic_bone_composition);
        PhysicalForm.BodyPart pelvic_tissue = new PhysicalForm.BodyPart(name: "Pelvic Tissue",
                                                                        components: pelvic_tissue_composition);
        PhysicalForm.BodyPart geldables = new PhysicalForm.BodyPart(name: "Geldables",
                                                                    components: geldables_composition);

        PhysicalForm.BodyPart[][] torso_layers;
        torso_layers = new PhysicalForm.BodyPart[][] { new PhysicalForm.BodyPart[] { spine },
                                                       new PhysicalForm.BodyPart[] { torso_tissue, heart } };
        PhysicalForm.BodyPart[][] pelvis_layers;
        pelvis_layers = new PhysicalForm.BodyPart[][] { new PhysicalForm.BodyPart[] { pelvic_bone },
                                                        new PhysicalForm.BodyPart[] { pelvic_tissue, geldables } };

        // Add the torso and pelvis to the test dummy and create a joint from the former to the latter
        body.Limbs = new PhysicalForm.Limb[] { new PhysicalForm.Limb(name: "Torso", layers: torso_layers),
                                               new PhysicalForm.Limb(name: "Pelvis", layers: pelvis_layers) };
        body.Joints = new PhysicalForm.LimbJoint[] { new PhysicalForm.LimbJoint(origin_name: "Torso",
                                                                                dependent_name: "Pelvis") };

        yield return null;
    }

    /// <summary>
    /// Tests that a newly created GameObject can be assigned a PhysicalForm and given a valid set of limbs and BodyParts. 
    /// </summary>
    [UnityTest]
    public IEnumerator TestCreation()
    {
        Assert.That(((PhysicalForm)billy_bob.GetComponent("PhysicalForm")).Limbs[0].Layers[0].Length != 0);
        yield return null;
    }

    /// <summary>
    /// Asserts that TakeDamage with a valid input causes a change in the total integrity of the PhysicalForm.
    /// </summary>
    [UnityTest]
    public IEnumerator TestTakeDamage()
    {
        PhysicalForm form = (PhysicalForm)billy_bob.GetComponent("PhysicalForm");
        float[] limb_sums = (from limb in form.Limbs
                             select (from layer in limb.Layers
                                     select (from part in layer
                                             select part.Integrity).Sum()).Sum()).ToArray();
        float initial_integrity = limb_sums.Sum();

        form.TakeDamage(new PhysicalForm.BodyPartDamage(impact: 5, shear: 5, corrosive: 5, energy: 5));
        limb_sums = (from limb in form.Limbs
                     select (from layer in limb.Layers
                             select (from part in layer
                                     select part.Integrity).Sum()).Sum()).ToArray();

        float final_integrity = limb_sums.Sum();

        Assert.AreNotEqual(initial_integrity, final_integrity);

        yield return null;
    }
}