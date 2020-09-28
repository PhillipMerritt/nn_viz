using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Diagnostics;
using System;


public class MainMenu : MonoBehaviour
{
    public string modelPath;
    public string inputPath;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void play ()
    {
        //Assets/Resources/pythonVenv/Scripts/python.exe
        //Assets/Resources/getActivations.py
        string python_path = $"{Application.dataPath}/Resources/pythonVenv/Scripts/python.exe"
        string args = $"{Application.dataPath}/Resources/getActivations.py {modelPath} {inputPath}";
        print(args);
        try
        {
            Process p1 = new Process();
            p1.StartInfo = new ProcessStartInfo(python_path, args);
            p1.StartInfo.UseShellExecute = false;
            p1.StartInfo.RedirectStandardOutput = true;
            p1.Start();
            p1.WaitForExit();  
        }
        catch (Exception ex)
        {
            print("There is a problem in your Python code: " + ex.Message);
        }
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void getModel ()
    {
        modelPath = EditorUtility.OpenFilePanel("Select model file", "", ".h5");
        print(modelPath);
    }

    public void getInputs ()
    {
        inputPath = EditorUtility.OpenFilePanel("Select input", "", "");
        print(inputPath);
    }

    public void options ()
    {

    }

    public void quit ()
    {

    }
}
