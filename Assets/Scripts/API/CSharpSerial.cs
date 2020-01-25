using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

public class CSharpSerial : MonoBehaviour {

    private bool notDongle = false;                             // True if dongle is not connected when the application initialises.
    private bool notDongleTriggered = false;                    // True if dongle notification has already been triggered by the system.
    Thread thread;                                              // Separate thread used to read bytes data from the dongle.
    public string serialPort;                                   // Serial port name used to connect to the dongle.
    public int baudRate;                                        // Baud rate number used to establish the connection to the dongle.

    Quaternion quat;                                            // Base quaternion class to calculate the rotation of the hands.
    public GameObject quatObj;                                  // GameObject to pass the quaternions for rotation.
    public GameObject eulerObj;                                 // GameObject to pass the quaternions for euler angles.
    public string initialiseCommand;                            // Initialise command to reset the signal.
    float[] lastRot = { 0, 0, 0 };                              // Need the last rotation to tell how far to spin the camera
    SerialPort stream;                                          // Stream object for connecting the UI to the dongle. This connection is being handle in a different thread.
    private int count = 0;                                      // Bytes internal counter.
    public int bufferSize;                                      // Max amount of bytes to stack in the buffer.
    // public HandController leftHand;                             // Lefthand class where the data read from the dongle is sent.
    // public HandController rightHand;                            // Righthand class where the data read from the dongle is sent.

    private Queue leftQueue; // From Unity to Arduino           // Left device queue to stack the data read.
    private Queue rightQueue;                                   // Right device queue to stack the data read.
    public Queue sendCommandQueueLeft;                          // Left device queue to stack the commands to send to the device.
    public Queue sendCommandQueueRight;                         // Right device queue to stack the commands to send to the device.
    public bool looping = true;                                 // Flag to control the loop for the read thread.                              
    public int disconnectedThreshold = 55;                      // Threshold to check wheter there is no data coming from 1 hand.                     

    private bool leftConnected = false;                             // Flag - true only when the left hand is connected.
    private bool rightConnected = false;                            // Flag - true only when the right hand is connected.
    private bool dongleConnected = false;                           // Flag - true only when the dongle is connected and detected by the UI.

    private int rightCounter;                                       // Internal counter to check if no data is coming from the right hand.
    private int leftCounter;                                        // Internal counter to check if no data is coming from the left hand.

    private bool sendVibrationToLeft = false;                       // Flag to check whether we send vibration to left hand device.
    private bool sendVibrationToRight = false;                      // Flag to check whether we send vibration to right hand device.
    private bool checkPorts = false;                                // Flag to control whether the dongle ports can be checked by the UI ( because in initialization the port has not serial metods available )
    private int os;                                                 // This variable checks the current user operative system. Then it is used to dinamycally get the dongle port name.

    // commands to be sent to the device - they are used for program logic purposess ( vibration, calibration ).
    private string startCalibrationCommand = "BP+CG";
    private string finishCalibrationCommand = "BP+CS";
    private string cancelCalibrationCommand = "BP+CC";
    private string resetOrientationCommand = "BP+RB";
    private string resetFingerDataCommand = "BP+RT";

    // profile commands - they are used to switch connection profiles.
    private string mouseCommand = "AT+MA";
    private string keyboardCommand = "AT+KA";
    private string gamepadCommand = "AT+GA";
    private string eteeCommand = "AT+NA";

    private string boom = "BP+VRL=88";

    // new etee API objects.
    public eteeDevice leftDevice;                                   // etee api left device.
    public eteeDevice rightDevice;                                  // etee api right device.


    // Start is called before the first frame update
    void Start() {
        Init();   

    }

    // Update is called once per frame
    void Update() {
        
        // check if there is data to send to the devices in the queues.
        // change this method by any used in your app logic.
        ReadingQueues();

        // check if vibration queues are overloaded.
        CheckVibrationQueuess();

        // check if we send vibration to left hand.
        if ( sendVibrationToLeft) {
            SendVibrationCommand( "left" );
        }

        //if(Input.GetKeyDown(KeyCode.Space))
        //    SendCommandToDevice(boom);

        // check if we send vibration to right hand.
        if ( sendVibrationToRight ) {
            SendVibrationCommand( "right" );
        }

        // check ports status.
        CheckPorts();
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    /// <returns>void</returns>
    void FixedUpdate() {
        
        // update no device activity counters.
        UpdateDisconnectedCounters();
    }

    /// <summary>
    /// Init class method.
    /// </summary>
    /// <returns>void</returns>
    private void Init() {

        // get current user operative system to detect the port where the dongle is connected.
        os = ( int ) System.Environment.OSVersion.Platform;

        // start separate thread to read data from the dongle.
        StartThread();
        
        // set internal flags to check when the devices are connected.
        leftConnected = false;
        rightConnected = false;

        // set data counter to initial value.
        count = 0;
    }

    /// <summary>
    /// Check if dongle is 
    /// connected.
    /// </summary>
    /// <returns>bool</returns>
    public bool IsDongleConnected() {
        return this.dongleConnected;
    }

    /// <summary>
    /// Check if device is connected
    /// Pass 0 for left device and
    /// 1 for right device.
    /// </summary>
    /// <param name="device">int - device id. Use 0 for left and 1 for right.</param>
    /// <returns>void</returns>
    public bool IsDeviceConnected( int device ) {
        
        // check if parameter is correct.
        if ( device > 1 ) {
            return false;
        }

        return ( device == 0 ) ? this.leftConnected : this.rightConnected;
    }
    /// <summary>
    /// Separate thread to read data from the
    /// dongle and sent to the queues.
    /// Then the system reads from the queues
    /// and triggers the logic.
    /// </summary>
    /// <returns>void</returns>
    private void StartThread() {

        // queues to read data from the device.
        leftQueue = Queue.Synchronized( new Queue() );
        rightQueue = Queue.Synchronized( new Queue() );

        // queues to send commands to the devices.
        sendCommandQueueLeft = Queue.Synchronized( new Queue() );
        sendCommandQueueRight = Queue.Synchronized( new Queue() );

        // start separate thread to read the data from the dongle - ThreadLoop is the method passed to the new thread.
        thread = new Thread( ThreadLoop );
        thread.Start();
    }

    /// <summary>
    /// Check the port based on 
    /// the user operative system.
    /// </summary>
    /// <returns>void</returns>
    private void SetPort() {
        int[] winCheckers = { 0, 1, 2, 3 };
        int[] unixCheckers = { 4, 6, 128 };

        if ( winCheckers.Contains( os ) ) {
            // set port for windows operative systems.
            SerialPortWindows();
        } else if ( unixCheckers.Contains( os ) ) {
            // set port for mac and linux operative systems.
            SerialPortUnix();
        }

    }

    /// <summary>
    /// Send command to the device.
    /// </summary>
    /// <param name="command">string -  command to be sent</param>
    /// <returns>void</returns>
    private void SendCommandToDevice( string command ) {
        if ( stream.IsOpen ) {
            stream.WriteLine( command );
            stream.BaseStream.Flush();
        }
    }

    /// <summary>
    /// Send start calibration command
    /// to the device.
    /// </summary>
    /// <returns>void</returns>
    public void SendStartCalibrationCommand() {
        this.SendCommandToDevice( this.startCalibrationCommand );
    }

    /// <summary>
    /// Send complete calibration command
    /// to the device.
    /// </summary>
    /// <returns>void</returns>
    public void SendCompleteCalibrationCommand() {
        this.SendCommandToDevice( this.finishCalibrationCommand );
    }

    /// <summary>
    /// Send cancel calibration command
    /// to the device.
    /// </summary>
    /// <returns>void</returns>
    public void SendCancelCalibrationCommand() {
        this.SendCommandToDevice( this.cancelCalibrationCommand );
    }

    /// <summary>
    /// Send reset orientation during calibration command
    /// to the device.
    /// </summary>
    /// <returns>void</returns>
    public void SendResetOrientationCalibrationCommand() {
        this.SendCommandToDevice( this.resetOrientationCommand );
    }

    /// <summary>
    /// Send reset finger data command to the device.
    /// </summary>
    /// <returns>void</returns>
    public void SendResetFingerDataCommand() {
        this.SendCommandToDevice( this.resetFingerDataCommand );
    }

    /// <summary>
    /// Send control profile
    /// command.
    /// </summary>
    /// <param name="profile">string - profile command name to be sent</param>
    /// <returns>void</returns>
    public void SendProfileCommand( string profile ) {

        switch ( profile ) {
            case "mouse":
                this.SendCommandToDevice( this.mouseCommand );
                break;
            case "keyboard":
                this.SendCommandToDevice( this.keyboardCommand );
                break;
            case "gamepad":
                this.SendCommandToDevice( this.gamepadCommand );
                break;
            case "etee":
                this.SendCommandToDevice( this.eteeCommand );
                break;
        }
        
    }

    /// <summary>
    /// Send Payload command.
    /// </summary>
    /// <param name="device">int - from which device the command is sent. Use 0 for left and 1 for right.</param>
    /// <param name="payload">string - payload to be sent.</param>
    /// <returns>void</returns>
    public void SendPayloadCommand( int device, string payload ) {
        string message = "BP+FP=";

        // set device.
        message += ( device == 0 ) ? "L" : "R";

        // add payload message.
        message += payload;

        // send command.
        this.SendCommandToDevice( message );
    }
 
    /// <summary>
    /// Reading queues.
    /// Read byte data from the queue and
    /// send data to the hand controllers.
    /// </summary>
    /// <returns>void</returns>
    private void ReadingQueues() {
        byte[] data;

        // read enqueue data for the right device.
        if ( rightQueue.Count > 0 ) {

            // update right device status.
            rightConnected = true;

            // build the data and send to the righr hand class.
            for ( int i = 0; i < rightQueue.Count; i++ ) {
                data = ( byte[] ) rightQueue.Dequeue();

                // etee api device
                rightDevice.GetRawData( data );
            }

            // update right counter to detect wheter the right device is sleeping.
            rightCounter = 0;
        }

        // read enqueue data for the left device.
        if ( leftQueue.Count > 0 ) {

            // update left device status.
            leftConnected = true;

            // build the data and send to the right hand class.
            for ( int i = 0; i < leftQueue.Count; i++ ) {
                data = ( byte[] ) leftQueue.Dequeue();

                // etee api device.
                leftDevice.GetRawData( data );
            }

            // update left counter to detect wheter the left device is sleeping.
            leftCounter = 0;
        }
    }



    /// <summary>
    /// Read bytes data from the dongle
    /// and send to the queues for each device.
    /// This method is pemanently running
    /// in the separate thread.
    /// </summary>
    /// <returns>void</returns>
    private void ThreadLoop() {

        // set the port.
        SetPort();
        
        // set arrays for reading and storing byte data from the devices.
        byte[] serialData = new byte[ 9999 ];
        byte[] lastByte = new byte[1];

        // set extra data variables.
        int bufferLimit = 255;
        bool is_right = false;
        
        // ensure the counter is initialized to 0.
        count = 0;

        // open the stream reading from the port.
        stream = new SerialPort( serialPort, baudRate );
        stream.Encoding = System.Text.Encoding.UTF8;
        stream.Open();

        // separate thread loop to continuosly read data from the device.
        while ( looping ) {
            
            while ( stream.BytesToRead > 0 ) {

                // read data from the dongle.
                // get the last byte and save into the data stack.
                stream.Read( lastByte, 0, 1 );
                serialData[ count ] = lastByte[0];


                // The end of each package is set when the device sends
                // two 255 in a row. If that happens, the limit
                // of the package has been reached.
                if ( count > 0 && serialData[ count ] == bufferLimit && serialData[ count - 1 ] == bufferLimit ) {

                    if ( count == bufferSize ) {
                        
                        // The 16 position in the bytes contains the
                        // bits for general information. To detect from
                        // which device the data comes from, we check
                        // if the first bit is 0 or 1. 0 is for data
                        // coming from the left device and 1 is data
                        // coming from the right device.
                        is_right = IsBitSet( serialData[16], 0 );

                        // enqueue device data.
                        if ( is_right ) {
                            rightQueue.Enqueue( serialData );
                        } else {
                            leftQueue.Enqueue( serialData );
                        }

                        // reset data stack to receive more data packages.
                        serialData = new byte[9999];
                        count = 0;
                        stream.BaseStream.Flush();

                    } else {

                        // reset data stack to receive more data packages.
                        serialData = new byte[9999];
                        count = 0;
                        stream.BaseStream.Flush();
                    }
                } else {
                    count++;
                }
                
            }
            

           stream.BaseStream.Flush();
        }

        // stop the thread if the while statement is broken and close connection to the port.
        StopThread();

        if ( stream.IsOpen ) {
            stream.Close();
        }
    }

    /// <summary>
    /// Stop the spearate thread.
    /// </summary>
    /// <returns>void</returns>
    public void StopThread() {
        lock ( this ) {
            looping = false;
        }
    }

    /// <summary>
    /// Send vibration command
    /// to the devices.
    /// </summary>
    /// <param name="hand">string - to which hand the vibration command is being sent</param>
    /// <returns>void</returns>
    private void SendVibrationCommand( string hand ) {
        
        // commannd for the left hand device.
        if ( hand == "left" ) {
            string command1 = ( string ) sendCommandQueueLeft.Dequeue();

            sendVibrationToLeft = false;
            sendCommandQueueLeft.Clear();

            stream.WriteLine( command1 );
        }

        if ( hand == "left" ) {
            string command2 = ( string ) sendCommandQueueRight.Dequeue();

            sendVibrationToRight = false;
            sendCommandQueueRight.Clear();

            stream.WriteLine( command2 );
        }
    }

    /// <summary>
    /// Release items from vibration queue.
    /// </summary>
    public void CheckVibrationQueuess() {

        if ( ! leftConnected && sendCommandQueueLeft.Count > 0 ) {
            sendCommandQueueLeft.Clear();
        }
 
        if ( ! rightConnected && sendCommandQueueRight.Count > 0 ) {
            sendCommandQueueRight.Clear();
        }
    }

    /// <summary>
    /// Update flags to check wheter the
    /// devices are connected or disconnected.
    /// </summary>
    /// <returns>void</returns>
    private void UpdateDisconnectedCounters() {
        rightCounter++;
        leftCounter++;
    }

    /// <summary>
    /// Check existing ports and check if
    /// the dongle is connected.
    /// </summary>
    /// <returns>void</returns>
    public void CheckPorts() {

        if ( checkPorts ) {

            // check if the dongle is connected.
            if ( SerialPort.GetPortNames().Length > 0 ) {
                dongleConnected = true;
            } else {
                dongleConnected = false;
            }
        }
    }

    /// <summary>
    /// Get active dongle port for
    /// windows systems.
    /// </summary>
    /// <returns>void</returns>
    private void SerialPortWindows() {
        
        List<string> names = ComPortNames( "10C4", "EA60" );

        if ( names.Count > 0 ) {
            checkPorts = true;

            if ( SerialPort.GetPortNames().Length == 0 ) {
                Debug.LogWarning( "Tg0 dongle not detected, please connect it and restart" );
                notDongle = true;
            } else {

                foreach ( string s in SerialPort.GetPortNames() ) {
                    for ( int i = 0; i < names.Count; i++ ) {

                        if ( s.Contains( names[i] ) ) {
                            serialPort = s;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get active dongle port for
    /// unix systems ( max, linux )
    /// </summary>
    /// <returns>void</returns>
    private void SerialPortUnix() {

        List<string> serialPorts = new List<string>();

        string[] cu = Directory.GetFiles( "/dev/", "cu.SLAB_USBtoUART" );
        
        foreach ( string dev in cu ) {
            if ( dev.StartsWith( "/dev/cu.SLAB_USBtoUART" ) ) {
                serialPorts.Add( dev );
            }
        }

        if ( serialPorts.Count == 1 ) {
            serialPort = serialPorts[0];
        }
    }

    /// <summary>
    /// Get list of com ports names
    /// availables for the dongle to use.
    /// </summary>
    /// <param name="VID">string - 16-bit vendor number ( Vendor ID )</string>
    /// <param name="PID">string - 16-bit product number ( Product ID )</string>
    /// <returns>List</returns>
    List<string> ComPortNames( string VID, string PID ) {
        string pattern = string.Format( "^VID_{0}.PID_{1}", VID, PID );
        Regex _rx = new Regex( pattern, RegexOptions.IgnoreCase );

        List<string> comports = new List<string>();

        RegistryKey rk1 = Registry.LocalMachine;
        RegistryKey rk2 = rk1.OpenSubKey( "SYSTEM\\CurrentControlSet\\Enum" );

        foreach ( string s3 in rk2.GetSubKeyNames() ) {
            RegistryKey rk3 = rk2.OpenSubKey(s3);

            foreach ( string s in rk3.GetSubKeyNames() ) {

                if ( _rx.Match(s).Success ) {
                    RegistryKey rk4 = rk3.OpenSubKey(s);

                    foreach ( string s2 in rk4.GetSubKeyNames() ) {
                        RegistryKey rk5 = rk4.OpenSubKey(s2);
                        RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");

                        try {
                            string port = (string)rk6.GetValue("PortName");
                            if (port != null)
                                comports.Add(port);
                        }
                        catch { }
                    }
                }
            }
        }

        return comports;
    }

    /// <summary>
    /// Get the bit position inside
    /// a byte.
    /// Used here to get gesture and
    /// info values.
    /// </summary>
    /// <param name="b">byte - byte where we look for the bit</param>
    /// <param name="pos">int - which position of the byte we want to get</param>
    /// <returns>bool</returns>
    public bool IsBitSet( byte b, int pos ) {
        return ( b & ( 1 << pos ) ) != 0;
    }

    /// <summary>
    /// Event listener triggered
    /// when the application stops
    /// running.
    /// It stops the separate threads
    /// and close the connection to 
    /// the serial port.
    /// </summary>
    /// <returns>void</returns>
    public void OnApplicationQuit() {
        StopThread();

        if ( stream.IsOpen ) {
            stream.Close();
        }
    }
 
}
