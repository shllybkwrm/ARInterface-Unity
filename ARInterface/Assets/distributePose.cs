using System;
using UnityEngine;
using UnityEngine.UI;

public class distributePose : MonoBehaviour {
    private InputField thisField;
    public InputField MessageOutput;
    public int idx;

    private char[] poseComponents = {'x','y','z','r','p','y'};

	// Use this for initialization
	void Start () {
        thisField = this.GetComponent<InputField>();

        thisField.text = String.Format("{0}: 0", poseComponents[idx]);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
