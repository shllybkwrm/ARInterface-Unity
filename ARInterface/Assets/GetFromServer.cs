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
                //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                Debug.Log(String.Format("Connecting from {0}",
                        ipHostInfo.ToString()));
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                Debug.Log(String.Format("Connecting to {0}",
                        ipAddress.ToString()));
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 20602);

                path = ipAddress;
                port = 20602;

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
    }

    void TaskOnClick()
    {
        //Debug.Log("You have clicked the button!");


        // Encode the data string into a byte array.  
        byte[] msg = Encoding.ASCII.GetBytes("Get pose");
        string pose = crpiClient.sendMsg(msg);

        Debug.Log(String.Format("Click received pose: {0}", pose));

        MessageOutput.text = String.Format("{0}", pose);

    }


}
