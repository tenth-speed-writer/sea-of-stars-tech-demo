using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static object WeightedRandomDraw(object[] choices, float[] weights)
    {
        // Calculate an array of cumulative weights
        float[] cum_weights = { };
        for (int i = 0; i < weights.Length; i++)
        {
            cum_weights = cum_weights.Concat(new float[] { cum_weights.Sum() + weights[i] }).ToArray();
        }

        // Roll a random value in range [0, cum_weights.Max()].
        // r.NextDouble is uniform(0, 1), so we can just multiply by the max.
        System.Random r = new System.Random();
        float roll = (float)r.NextDouble() * cum_weights.Max();

        // Determine the smallest cum_weights value greater than the roll and get its index.
        // In this dynamic, a very small roll snaps up to the first element and a very
        // large one snaps up to the last element--so I hope it works! :)
        float[] larger_weights = (from cw in cum_weights where cw >= roll select cw).ToArray();
        float matching_cum_weight = larger_weights[0];
        int drawn_index = Array.FindIndex(cum_weights, val => val == matching_cum_weight);

        // Use that index to select the drawn choice and return it
        return choices[drawn_index];
    }

    public static object WeightedRandomDraw(PhysicalForm.BodyPart[] choices, float[] weights)
    {
        // Calculate an array of cumulative weights
        float[] cum_weights = { };
        for (int i = 0; i < weights.Length; i++)
        {
            cum_weights = cum_weights.Concat(new float[] { cum_weights.Sum() + weights[i] }).ToArray();
        }

        // Roll a random value in range [0, cum_weights.Max()].
        // r.NextDouble is uniform(0, 1), so we can just multiply by the max.
        System.Random r = new System.Random();
        float roll = (float)r.NextDouble() * cum_weights.Max();

        // Determine the smallest cum_weights value greater than the roll and get its index.
        // In this dynamic, a very small roll snaps up to the first element and a very
        // large one snaps up to the last element--so I hope it works! :)
        float[] larger_weights = (from cw in cum_weights where cw >= roll select cw).ToArray();
        float matching_cum_weight = larger_weights[0];
        int drawn_index = Array.FindIndex(cum_weights, val => val == matching_cum_weight);

        // Use that index to select the drawn choice and return it
        return choices[drawn_index];
    }

    public static object WeightedRandomDraw(PhysicalForm.Limb[] choices, float[] weights)
    {
        // Calculate an array of cumulative weights
        float[] cum_weights = { };
        for (int i = 0; i < weights.Length; i++)
        {
            //cum_weights[cum_weights.Length] = cum_weights.Sum() + weights[i];
            cum_weights = cum_weights.Concat(new float[] { cum_weights.Sum() + weights[i] }).ToArray();
        }

        // Roll a random value in range [0, cum_weights.Max()].
        // r.NextDouble is uniform(0, 1), so we can just multiply by the max.
        System.Random r = new System.Random();
        float roll = (float)r.NextDouble() * cum_weights.Max();

        // Determine the smallest cum_weights value greater than the roll and get its index.
        // In this dynamic, a very small roll snaps up to the first element and a very
        // large one snaps up to the last element--so I hope it works! :)
        float[] larger_weights = (from cw in cum_weights where cw >= roll select cw).ToArray();
        float matching_cum_weight = larger_weights[0];
        int drawn_index = Array.FindIndex(cum_weights, val => val == matching_cum_weight);

        // Use that index to select the drawn choice and return it
        return choices[drawn_index];
    }
}