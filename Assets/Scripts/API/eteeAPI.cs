using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eteeAPI : MonoBehaviour {
    public static eteeAPI instance;                                 // Static instance to make this API available in the whole application scope.
    public CSharpSerial serialRead;                                 // Serial reader class component reference.
    public eteeDevice leftDevice;                                   // etee left device from where the data is retrieved class component refernece.
    public eteeDevice rightDevice;                                  // etee right device from where the data is retrieved class componer reference.

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    /// <returns>void</returns>
    void Awake() {
        if ( instance == null ) {
            instance = this;
        }
    }

    /// <summary>
    /// Checks if the dongle is
    /// connected.
    /// </summary>
    /// <returns>bool</returns>
    public bool IsConnected() {
        return serialRead.IsDongleConnected();
    }

    /// <summary>
    /// Disconnects the system.
    /// Stops serial read reading
    /// data thread.
    /// </summary>
    /// <returns>void</returns>
    public void Disconnect() {
        serialRead.StopThread();
    }

    /// <summary>
    /// Checks if left device
    /// is connected.
    /// </summary>
    /// <returns>bool</returns>
    public bool IsLeftConnected() {
        // 0 is used for left device as standard in all the etee api library.
        return serialRead.IsDeviceConnected( 0 );
    }

    /// <summary>
    /// Check if right device
    /// is connected.
    /// </summary>
    /// <returns>bool</returns>
    public bool IsRightConnected() {
        // 1 us used for right device as standard in all the etee api library.
        return serialRead.IsDeviceConnected( 1 );
    }

    /// <summary>
    /// Get port name used
    /// to establish the connection.
    /// </summary>
    /// <returns>string</returns>
    public string GetPortName() {
        return serialRead.serialPort;
    }

    /// <summary>
    /// Get single finger data.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <parma name="fingerIndex">int - finger index. The correlation is the following: 0 - Thumb, 1 - Index, 2 - Middle, 3 - Ring, 4 - Pinky</param>
    /// <returns>float</returns>
    public float GetFinger( int device, int fingerIndex ) {

        float value = 0f;

        // check that the value requested is valid.
        if ( fingerIndex < 0 || fingerIndex > 4 ) {
            return value;
        }

        switch ( fingerIndex ) {
            case 0:
                value = ( device == 0 ) ? leftDevice.thumb : rightDevice.thumb;
                break;
            case 1:
                value = ( device == 0 ) ? leftDevice.index : rightDevice.index;
                break;
            case 2:
                value = ( device == 0 ) ? leftDevice.middle : rightDevice.middle;
                break;
            case 3:
                value = ( device == 0 ) ? leftDevice.ring : rightDevice.ring;
                break;
            case 4:
                value = ( device == 0 ) ? leftDevice.pinky : rightDevice.pinky;
                break;
            default:
                value = 0f;
                break;
        }

        return value;
    }

    /// <summary>
    /// Get all fingers data.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>float[]</returns>
    public float[] GetAllFingers( int device ) {

        if ( device > 1 ) {
            float[] nullOp = new float[1];
            return nullOp;
        }

        return ( device == 0 ) ? leftDevice.fingerData : rightDevice.fingerData;
    }

    /// <summary>
    /// Get trackpad axis value.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <param name="axis">char - axis whose value you want to retrieve.true Values allowed are 'x' or 'y'</param>
    /// <returns>float</returns>
    public float GetTrackpadSingleAxis( int device, char axis ) {
        // check that the value requested is valid.
        if ( axis != 'x' && axis != 'y' ) {
            return 0f;
        }

        Vector2 data = ( device == 0 ) ? leftDevice.trackPadCoordinates : rightDevice.trackPadCoordinates;
        return ( axis == 'x' ) ? data.x : data.y;
    }

    /// <summary>
    /// Get trackpad axis values.
    /// </sumamry>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>Vector2</returns>
    public Vector2 GetTrackPadAxis( int device ) {
        
        // check that parameter is correct.
        if ( device > 1 ) {
            return new Vector2( 0f, 0f );
        }

        return ( device == 0 ) ? leftDevice.trackPadCoordinates : rightDevice.trackPadCoordinates;
    }

    /// <summary>
    /// Get battery value.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>float</returns>
    public float GetBattery( int device ) {
        
        // check the parameter is correct.
        if ( device > 1 ) {
            return 0f;
        }

        return ( device == 0 ) ? leftDevice.battery : rightDevice.battery;
    }

    /// <summary>
    /// Get single quaternion component
    /// value for rotation.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <param name="component">char - component name. Values allowed are: 'x', 'y', 'z' and 'w'</param>
    /// <returns>float</returns>
    public float GetQuaternionComponent( int device, char component ) {

        float value = 0f;
        
        // check that parameter device is correct.
        if ( device > 1 ) {
            return 0f;
        }

        Quaternion data = ( device == 0 ) ? leftDevice.quaternions : rightDevice.quaternions;

        switch ( component ) {
            case 'x':
                value = data.x;
                break;
            case 'y':
                value = data.y;
                break;
            case 'z':
                value = data.z;
                break;
            case 'w':
                value = data.w;
                break;
            default:
                value = 0f;
                break;
        }

        return value;
    }

    /// <summary>
    /// Get quaternions data
    /// values for rotation.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>Quaternion</returns>
    public Quaternion GetQuaternions( int device ) {

        // check that device number is correct.
        if ( device > 1 ) {
            return new Quaternion( 0f, 0f, 0f, 0f );
        }

        return ( device == 0 ) ? leftDevice.quaternions : rightDevice.quaternions;
    }

    /// <summary>
    /// Get acceleromenter axis
    /// data for velocity.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <parma name="axis">char - Axis to be retrieved. Allowed values are: 'x', 'y', 'z'</param>
    /// <returns>float</returns>
    public float GetAccelerometerSingleAxis( int device, char axis ) {
        float value = 0f;

        // check that the device parameter is correct.
        if ( device > 1 ) {
            return value;
        }

        Vector3 data = ( device == 0 ) ? leftDevice.accelerometer : rightDevice.accelerometer;

        switch ( axis ) {
            case 'x':
                value = data.x;
                break;
            case 'y':
                value = data.y;
                break;
            case 'z':
                value = data.z;
                break;
            default:
                value = 0f;
                break;
        }

        return value;
    }

    /// <summary>
    /// Get acceleromenter data
    /// for velocity.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>Vector3</returns>
    public Vector3 GetAccelerometerAxis( int device ) {
        
        // check that device parameter is correct.
        if ( device > 1 ) {
            return new Vector3( 0f, 0f, 0f );
        }

        return ( device == 0 ) ? leftDevice.accelerometer : rightDevice.accelerometer;
    }

    /// <summary>
    /// Check if tap has been
    /// performed by the user.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>void</returns>
    public bool IsTap( int device ) {
        
        // check that device parameter is correct.
        if ( device > 1 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.tap : rightDevice.tap;
    }

    /// <summary>
    /// Check if the LED
    /// button is pressed.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>bool</returns>
    public bool IsLedButton( int device ) {
        
        // check that device parameter is correct.
        if ( device > 1 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.ledButton : rightDevice.ledButton;
    }

    /// <summary>
    /// Check if clock wise
    /// rotation gesture has
    /// been performed in the
    /// trackpad.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>bool</returns>
    public bool IsCwScroll( int device ) {
        
        // check that device number is correct.
        if ( device > 1 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.cwScroll : rightDevice.cwScroll;
    }

    /// <summary>
    /// Check if anti-clock wise
    /// rotation gesture has been
    /// performed in the
    /// trackpad.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>void</returns>
    public bool IsAcwScroll( int device ) {
        
        // check that device number if correct.
        if ( device > 1 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.acwScroll : rightDevice.acwScroll;
   }

   /// <summary>
    /// Checks if a squeeze
    /// gesture is being performed
    /// by the user.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>bool</returns>
    public bool IsSqueeze( int device ) {

        // check that the device number is correct.
        if ( device > 1 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.squeeze : rightDevice.squeeze;
    }

    /// <summary>
    /// Check if the device
    /// is in horizontal position.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>bool</returns>
    public bool InHorizontal( int device ) {

        // check that device number is correct.
        if ( device > 1 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.horizontal : rightDevice.horizontal;
    }

    /// <summary>
    /// Check if the device
    /// is in vertical position.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>void</returns>
    public bool InVertical( int device ) {

        // check that device number is correct.
        if ( device > 1 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.vertical : rightDevice.vertical;
    }

    /// <summary>
    /// Check if the user
    /// is performing a point
    /// gesture.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>bool</returns>
    public bool IsPointGesture( int device ) {

        // check if device number is correct.
        if ( device == 0 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.point : rightDevice.point;
    }

    /// <summary>
    /// Check if the user 
    /// is performing a pinch
    /// gesture.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>bool</returns>
    public bool IsPinchGesture( int device ) {

        // check if device number is correct.
        if ( device == 0 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.pinch : rightDevice.pinch;
    }

    /// <summary>
    /// Check if the user is
    /// performing a left
    /// slap gesture.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>void</returns>
    public bool IsSlapLeft( int device ) {

        // check if device number is correct.
        if ( device == 0 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.slapLeft : rightDevice.slapLeft;
    }

    /// <summary>
    /// Check if the user is
    /// performing a right slap
    /// gesture.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <returns>void</returns>
    public bool IsSlapRight( int device ) {

        // check if device number is correct.
        if ( device == 0 ) {
            return false;
        }

        return ( device == 0 ) ? leftDevice.slapRight : leftDevice.slapRight;
    }

    /// <summary>
    /// Send Payload command.
    /// </summary>
    /// <param name="device">int - device number. Use 0 for left and 1 for right</param>
    /// <param name="message">string - message to be sent.</param>
    /// <returns>void</returns>
    public void SendPayLoadCommand( int device, string message ) {
        serialRead.SendPayloadCommand( device, message );
    }

}
