using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System;
using TMPro;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    public string modelPath = "C:/Users/phill/Downloads/cnn.h5";
    public string inputPath = "C:/UnityProjects/nnViz/Assets/inputs/img_1.jpg";

    public List<int> tempShape;
    public bool tempNormalize = true;
    public List<string> tempLabels;

    // Start is called before the first frame update
    void Start()
    {
        modelPath = "C:/Users/phill/Downloads/cifar10cnn.h5";
        inputPath = "C:/Users/phill/Downloads/inputs";
        string[] temp = new string[]{"airplane", "automobile", "bird", "cat", "deer", "dog", "frog", "horse", "ship", "truck"};
        tempLabels = new List<string>();
        foreach (string label in temp)
            tempLabels.Add(label);

        tempShape = new List<int>();
        foreach (int i in new int[]{32,32,3})
            tempShape.Add(i);
        
        save();
    }

    public void Awake() {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static string GetProjectPath()
     {
         var args = System.Environment.GetCommandLineArgs();
         for (int i = 0; i < args.Length; i++)
         {
             if (args[i].Equals("-projectpath", System.StringComparison.InvariantCultureIgnoreCase)) return args[i + 1];
         }
         return string.Empty;
     }

    public void play ()
    {
        //Assets/Resources/pythonVenv/Scripts/python.exe
        //Assets/Resources/getActivations.py
        //string python_path = $"{Application.dataPath}/Resources/pythonVenv/Scripts/python.exe";
        //string args = $"{Application.dataPath}/Resources/getActivations.py";
        //print(args);
        print("Sending paths");
        //sideChannel.SendPathsToPython(modelPath, inputPath);

        print($"{GetProjectPath()}/Assets/Resources/getActivations.py");
        print($"{GetProjectPath()}/python-envs/env/Scripts/python.exe");

        run_cmd($"{GetProjectPath()}/Assets/Resources/getActivations.py", $"{modelPath} {inputPath}");
        print("Sent paths");
    }

    private void run_cmd(string cmd, string args)
    {
        Process p = new Process();
        //p.Exited += new EventHandler(nextScene);
        p.StartInfo = new ProcessStartInfo($"{GetProjectPath()}/python-envs/env/Scripts/python.exe", $"{cmd} {args}")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        print(output);

        while(!p.HasExited)
        {

        }
        nextScene();
    }

    private static void nextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void getModel ()
    {
        modelPath = EditorUtility.OpenFilePanel("Select model file", "", "H5");
        print(modelPath);
    }

    public void getInputs ()
    {
        inputPath = EditorUtility.OpenFolderPanel("Select input folder (only images)", "", "");
        print(inputPath);
    }

    public void quit ()
    {

    }

    public void updateShape(GameObject inputBox)
    {
        string input = inputBox.GetComponent<TMP_Text>().text;
        input = input.Trim();
        string[] string_vals = input.Split(',');

        tempShape = new List<int>();
        foreach (string dim in string_vals){
            input = new string(dim.Where(c => char.IsDigit(c)).ToArray());
            tempShape.Add(Int32.Parse(input));
        }
    }

    public void updateNormalize(GameObject inputBox)
    {
        tempNormalize = inputBox.GetComponent<Toggle>().isOn;
    }

    public void updateTags(GameObject inputBox)
    {
        string input = inputBox.GetComponent<TMP_Text>().text;
        input = input.Trim();
        string [] string_vals = input.Split(',');
        string_vals[string_vals.Length - 1] = new string(string_vals[string_vals.Length - 1].Where(c => char.IsLetter(c) || char.IsDigit(c) || c == ' ').ToArray());

        tempLabels = new List<string>();
        foreach (string label in string_vals)
            tempLabels.Add(label.Trim());
    }

    public void save ()
    {
        Settings.setSettings(tempShape, tempNormalize, tempLabels);
    }
}
