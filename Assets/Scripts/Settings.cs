using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static List<int> shape = new List<int>();
    public static bool normalize = true;
    public static List<string> labels;

    public static int timing;

    public static void setSettings(List<int> newShape, bool newNorm, List<string> newLabels)
    {
        shape = newShape;
        normalize = newNorm;
        labels = newLabels;
        timing = 3;
    }
}
