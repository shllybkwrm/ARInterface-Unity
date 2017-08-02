using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// From https://msdn.microsoft.com/en-us/library/kb5kfec7(v=vs.110).aspx
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


public class GetFromServer : MonoBehaviour {
    private Button thisButton;
    public InputField MessageOutput;
    public Dropdown CommandInput;


    public GameObject CanvasObj;
    private Slider[] sliderList = new Slider[6];


    // Class to store client info & methods
    public class client
    {
        public IPAddress path;
        public int port;
        public Boolean isConnected;

        private Socket sender;

        public client()
        {
            isConnected = false;
        }

        public client(IPAddress ip, int p)
        {
            path = ip;
            port = p;
            isConnected = false;
        }



        public string sendMsg(byte[] msg)
        {
            if (isConnected == false)
            {
                Debug.Log(String.Format("Error, client not connected.  Connecting now."));
                connect();
            }

            // Data buffer for incoming data. 
            byte[] bytes = new byte[1024];

            // Send the data through the socket.  
            int bytesSent = sender.Send(msg);

            // Receive the response from the remote device.  
            int bytesRec = sender.Receive(bytes);
            string msgRec = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            Debug.Log(String.Format("Echoed test = {0}", msgRec));

            return msgRec;

        }


        public void connect()
        {
            // Data buffer for incoming data.  
            //byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {

                // Establish the remote endpoint for the socket.  
                // The example uses port 11000 on the local computer.  
                //! Obsolete:  IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                Debug.Log(String.Format("Connecting from {0}",
                        ipHostInfo.ToString()));
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                byte[] ipBytes = { 169, 254, 152, 39 };
                IPAddress ipAddressRemote = new IPAddress(ipBytes);

                path = ipAddressRemote;
                port = 20602;

                Debug.Log(String.Format("Connecting to {0}",
                        path.ToString()));
                IPEndPoint remoteEP = new IPEndPoint(path, port);


                // Create a TCP/IP  socket.  
                sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {

                    if (isConnected == false) {
                    sender.Connect(remoteEP);

                    Debug.Log(String.Format("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString()));

                    isConnected = true;
                    }
                    
                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("Connected from Unity Server.");

                    sendMsg(msg);

                    // ---Where to do this??---
                    // Release the socket.  
                    //sender.Shutdown(SocketShutdown.Both);
                    //sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Debug.Log(String.Format("ArgumentNullException : {0}", ane.ToString()));
                }
                catch (SocketException se)
                {
                    Debug.Log(String.Format("SocketException : {0}", se.ToString()));
                }
                catch (Exception e)
                {
                    Debug.Log(String.Format("Unexpected exception : {0}", e.ToString()));
                }

            }
            catch (Exception e)
            {
                Debug.Log(String.Format(e.ToString()));
            }
        }

        public void disconnect()
        {
            if (isConnected)
            {
                // Release the socket.  
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

                isConnected = false;
            }
            
        }

    }

    // Create class to hold client details
    public client crpiClient = new client();


    void Start()
    {
        thisButton = this.GetComponent<Button>();
        //Button btn = yourButton.GetComponent<Button>();
        thisButton.onClick.AddListener(TaskOnClick);

        //GameObject IF = GameObject.Find("Canvas/Scaler/MessageOutput");
        //MessageOutput = IF.GetComponent<InputField>();


        // Encode the data string into a byte array.  
        //byte[] msg = Encoding.ASCII.GetBytes("Connected from Unity Server.");
        //crpiClient.connect();


        initializeSliders();
    }

    float timeCounter = 0.0f;

    void Update()
    {

        // Increment timer
        timeCounter += Time.deltaTime;

        if (crpiClient.isConnected && timeCounter>=10.0f)
        {
            Debug.LogFormat("Timer: {0}", timeCounter);


            // Live update axes  
            byte[] msg = Encoding.ASCII.GetBytes("Get Axes");
            string ans = crpiClient.sendMsg(msg);
            Debug.LogFormat("Robot Axes: {0}", ans);
            setSliders(ans);


            // Plot forces at every interval 
            msg = Encoding.ASCII.GetBytes("Get Forces");
            ans = crpiClient.sendMsg(msg);
            Debug.LogFormat("Robot Forces: {0}", ans);
            plotForces(ans);


            timeCounter = 0.0f;
        }

    }

    void TaskOnClick()
    {
        //Debug.Log("You have clicked the button!");
        String cmd = CommandInput.captionText.text;
        Debug.Log(cmd);

        //if (thisButton.name.Equals("GetPose")) 
        //    cmd = "Get Pose";

        // Encode the data string into a byte array.  
        byte[] msg = Encoding.ASCII.GetBytes(cmd);
        string ans = crpiClient.sendMsg(msg);


        if (cmd.Equals("Get Pose"))
        {
            Debug.Log(String.Format("Robot Pose: {0}", ans));
        }
        else if (cmd.Equals("Get Forces"))
        {
            Debug.Log(String.Format("Robot Forces: {0}", ans));
        }
        
        else if (cmd.Equals("Get Axes"))
        {
            Debug.LogFormat("Robot Axes: {0}", ans);
            setSliders(ans);
        }


        MessageOutput.text = String.Format("{0}", ans);

    }


    // Set sliders to joint values rcvd from robot
    void setSliders(string input)
    {
        input = input.Replace('(', ' ').Replace(')', ' ');
        //Debug.Log(input);
        Double[] axes = Array.ConvertAll(input.Split(','), Double.Parse);
        //Debug.LogFormat("Converted Axes: {0}...", axes[0]);

        double tempVal = 0.0;
        for (int i = 0; i < 6; i++)
        {
            // Offsets
            switch (i)
            {
                case 0:
                    tempVal = -1.0*(axes[i]+45.0);
                    break;
                case 1:
                    tempVal = axes[i] + 90.0;
                    break;
                case 3:
                    tempVal = axes[i] + 90.0;
                    break;
                case 4:
                    tempVal = -1.0*axes[i];
                    break;
                default:
                    tempVal = axes[i];
                    break;
            }

            // Check if out of bounds and loop around
            if (tempVal > 180.0)
            {
                tempVal = -180.0 + (tempVal - 180.0);
            }
            else if (tempVal < -180.0)
            {

                tempVal = 180.0 - (tempVal + 180.0);
            }

            // Set slider, which will set model
            sliderList[i].value = (float)tempVal;
        }
    }



    // Create the list of GameObjects that represent each slider in the canvas
    void initializeSliders()
    {
        var CanvasChildren = CanvasObj.GetComponentsInChildren<Slider>();

        for (int i = 0; i < CanvasChildren.Length; i++)
        {
            if (CanvasChildren[i].name == "Slider0")
            {
                sliderList[0] = CanvasChildren[i];
            }
            else if (CanvasChildren[i].name == "Slider1")
            {
                sliderList[1] = CanvasChildren[i];
            }
            else if (CanvasChildren[i].name == "Slider2")
            {
                sliderList[2] = CanvasChildren[i];
            }
            else if (CanvasChildren[i].name == "Slider3")
            {
                sliderList[3] = CanvasChildren[i];
            }
            else if (CanvasChildren[i].name == "Slider4")
            {
                sliderList[4] = CanvasChildren[i];
            }
            else if (CanvasChildren[i].name == "Slider5")
            {
                sliderList[5] = CanvasChildren[i];
            }
        }
    }



    // Set plot to updated force values
    public ParticleSystem plot;

    [Range(10, 100)]
    public int resolution = 10;

    private int currentResolution;
    private ParticleSystem.Particle[] points;

    private Double[] forceHistory = new Double[10];

    void plotForces(string input)
    {
        input = input.Replace('(', ' ').Replace(')', ' ');
        Double[] forces = Array.ConvertAll(input.Split(','), Double.Parse);


        if (currentResolution != resolution || points == null)
        {
            CreatePoints();
        }


        Array.Copy(forceHistory, 1, forceHistory, 0, forceHistory.Length-1);
        forceHistory[forceHistory.Length-1] = forces[0];
        Debug.LogFormat("forceHistory: {0}", forceHistory);


        for (int i = 0; i < resolution; i++)
        {
            Vector3 p = points[i].position;
            p.y = (float)forceHistory[i];
            points[i].position = p;
            Color c = points[i].color;
            c.g = p.y;
            points[i].color = c;
        }


        plot.GetComponent<ParticleSystem>().SetParticles(points, points.Length);
    }


    private void CreatePoints()
    {
        currentResolution = resolution;
        points = new ParticleSystem.Particle[resolution];
        float increment = 1f / (resolution - 1);
        for (int i = 0; i < resolution; i++)
        {
            float x = i * increment;
            points[i].position = new Vector3(x, 0f, 0f);
            points[i].color = new Color(x, 0f, 0f);
            points[i].size = 0.1f;
        }
    }


}
