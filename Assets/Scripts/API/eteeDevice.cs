using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eteeDevice : MonoBehaviour {
    public float thumb;                                         // Single value for thumb finger.
    public float index;                                         // Single value for index finger.
    public float middle;                                        // Single value for middle finger.
    public float ring;                                          // Single value for ring finger.
    public float pinky;                                         // Single value for pinky finger.
    public float[] fingerData = new float[5];                   // Finger data array.
    public Quaternion quaternions;                              // Quaternion values for rotation.
    public float battery;                                       // Battery value.
    public bool ledButton;                                      // Led button value.
    public bool tap;                                            // Tap value.
    public Vector2 trackPadCoordinates;                         // Trackpad vector2 coordiantes.
    public Vector3 accelerometer;                               // Accelerometer data.
    public bool cwScroll;                                       // Clockwise scroll in the trackpad gesture.
    public bool acwScroll;                                      // Anti clock wise scroll in the trackpad gesture.
    public bool squeeze;                                        // Squeeze gesture.
    public bool horizontal;                                     // Flag to check device horzontal position.
    public bool vertical;                                       // Flag to check device vertical position.
    public bool point;                                          // Point gesture.
    public bool pinch;                                          // Pinch gesture.
    public bool slapLeft;                                       // Flag to control left slap gesture.
    public bool slapRight;                                      // Flag to control right slap gesture.

    // start is called at the first frame before update.
    private void Start() {
        Init();
    }

    /// <summary>
    /// Update values from raw
    /// byte data.
    /// This method is called by
    /// the C# Serial Reader.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    public void GetRawData( byte[] serialBuffer ) {
        
        // update finger data.
        UpdateFingers( serialBuffer );

        // update quaternion values.
        UpdateRotation( serialBuffer );

        // update battery value.
        UpdateBatteryValue( serialBuffer );

        // update led value.
        UpdateLedButtonValue( serialBuffer );

        // update tap value.
        UpdateTapValue( serialBuffer );

        // update trackpad values.
        UpdateTrackPadValues( serialBuffer );

        // update accelerometer values.
        UpdateAccelerometerValues( serialBuffer );

        // update squeeze gesture.
        UpdateSqueezeGesture( serialBuffer );

        // update horizontal position value.
        UpdateHorizontalValue( serialBuffer );

        // update vertical position value.
        UpdateVerticalValue( serialBuffer );

        // update point gesture value.
        UpdatePointGesture( serialBuffer );

        // update pinch gesture value.
        UpdatePinchGesture( serialBuffer );

        // update anti clock wise trackpad rotation gesture value.
        UpdateAcwScrollGesture( serialBuffer );

        // update clock wise trackpad rotation gesture value.
        UpdateCwScrollGesture( serialBuffer );

        // update slap left gesture value
        UpdateSlapLeftGesture( serialBuffer );

        // update slap right gesture value.
        UpdateSlapRightGesture( serialBuffer );
    }

    /// <summary>
    /// Update fingers value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateFingers( byte[] serialBuffer ) {
        
        // fingers pressure are located in the 
        // bytes 0 to 4 in the serial buffer
        // array.
        for ( int i = ( this.fingerData.Length - 1 ); i >= 0; i-- ) {
            this.fingerData[ i ] = ( float ) serialBuffer[ i ];
        }

        // reverse array as fingers come from the backend inverted.
        // System.Array.Reverse( this.fingerData );

        // update fingers data.
        this.thumb = this.fingerData[0];
        this.index = this.fingerData[1];
        this.middle = this.fingerData[2];
        this.ring = this.fingerData[3];
        this.pinky = this.fingerData[4];
    }

    /// <summary>
    /// Update quaternion
    /// rotation values.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateRotation( byte[] serialBuffer ) {
        float normalizer = 100f;                // Normalize quaternions value.
        int gap = 0;                            // used to calculate the difference between the serial buffer array position and the quaternion data rotation.
        float[] rotationData = new float[4];    // used for temporal data calculation before adding the quaternion data to the array.

        // quaternions sbyte and float data.
        sbyte w;
        sbyte x;
        sbyte y;
        sbyte z;
        float qw = 0f;
        float qx = 0f;
        float qy = 0f;
        float qz = 0f;

        // get raw data for quaternions. This data is located in the bytes
        // 7 to 10 in the serial buffer array.
        for ( int i = 7; i <= 10; i++ ) {
            gap = i - 7;
            rotationData[ gap ] = ( float ) serialBuffer[ i ];

            // byte data comes from the device in values from 0
            // to 255, so in order to get the values we need in
            // Unity we have to convert to sbyte and then to float.
            switch ( gap ) {
                case 0:
                    w = unchecked( ( sbyte ) serialBuffer[ i ] );
                    qw = ( float ) ( w / normalizer );
                    break;
                case 1:
                    x = unchecked( ( sbyte ) serialBuffer[ i ] );
                    qx = ( float ) ( x / normalizer );
                    break;
                case 2:
                    y = unchecked( ( sbyte ) serialBuffer[ i ] );
                    qy = ( float ) ( y / normalizer );
                    break;
                case 3:
                    z = unchecked( ( sbyte ) serialBuffer[ i ] );
                    qz = ( float ) ( z / normalizer );
                    break;
            }
        }

        this.quaternions = new Quaternion( qy, -qz, -qx, qw );
    }

    /// <summary>
    /// Update battery value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateBatteryValue( byte[] serialBuffer ) {
        // battery values is located in the byte 14 in the serial buffer array.
        this.battery = ( int ) serialBuffer[ 14 ];
    }

    /// <summary>
    /// Update led button pressed
    /// value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateLedButtonValue( byte[] serialBuffer ) {
        // led button value is located at the 3rd bit in the 16th byte in the serial buffer array.
        this.ledButton = IsBitSet( serialBuffer[ 16 ], 3 );
    }

    /// <summary>
    /// Update tap action value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateTapValue( byte[] serialBuffer ) {
        // tap value is located at the 2nd bit in the 16th byte in the serial buffer array.
        this.tap = IsBitSet( serialBuffer[ 16 ], 2 );
    }

    /// <summary>
    /// Update trackPad coordinates.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateTrackPadValues( byte[] serialBuffer ) {
        // trackpad - coordinates data is in the byte 5 for the x value
        // and the byte 6 for the y value.
        sbyte xValue = unchecked( ( sbyte ) serialBuffer[ 5 ] );
        sbyte yValue = unchecked( ( sbyte ) serialBuffer[ 6 ] );

        this.trackPadCoordinates = new Vector2( ( float ) xValue, ( float ) yValue );
    }

    /// <summary>
    /// Update accelerometer data.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateAccelerometerValues( byte[] serialBuffer ) {
        int gap = 0;                                    // used to calculate the gap between the accelerometer data array and the serial buffer array positions.
        float[] accelerometerData = new float[3];       // temporal array for accelerometer data - used for data conversion.

        // accelerometer data is located in the positions from 11 to 13 in the serial buffer array.
        for ( int i = 11; i <= 11; i++ ) {
            gap = i - 11;
            accelerometerData[ gap ] = ( float ) serialBuffer[ i ];
        }

        // convert data and build 3D vector.
        this.accelerometer = new Vector3( unchecked( ( sbyte ) accelerometerData[ 0 ] ), 
                                          unchecked( ( sbyte ) accelerometerData[ 1 ] ), 
                                          unchecked( ( sbyte ) accelerometerData[ 2 ] ) );
    }

    /// <summary>
    /// Get Anticlockwise 
    /// scroll gesture from the
    /// trackpad.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateAcwScrollGesture( byte[] serialBuffer ) {
        this.acwScroll = IsBitSet( serialBuffer[16], 4 );
    }

    /// <summary>
    /// Get clockwise scroll
    /// gesture from the trackpad.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateCwScrollGesture( byte[] serialBuffer ) {
        this.cwScroll = IsBitSet( serialBuffer[16], 5 );
    }

    /// <summary>
    /// Get squeeze gesture data.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>bool</returns>
    private void UpdateSqueezeGesture( byte[] serialBuffer ) {
        this.squeeze = IsBitSet( serialBuffer[15], 0 );
    }

    /// <summary>
    /// Get horizontal
    /// position status value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateHorizontalValue( byte[] serialBuffer ) {
        this.horizontal = IsBitSet( serialBuffer[15], 4 );
    }

    /// <summary>
    /// Get vertical position
    /// status value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdateVerticalValue( byte[] serialBuffer ) {
        this.vertical = IsBitSet( serialBuffer[15], 4 );
    }

    /// <summary>
    /// Get point gesture
    /// status value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    private void UpdatePointGesture( byte[] serialBuffer ) {
        this.point = IsBitSet( serialBuffer[15], 1 );
    }

    /// <summary>
    /// Get pinch gesture
    /// status value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    public void UpdatePinchGesture( byte[] serialBuffer ) {
        this.pinch = IsBitSet( serialBuffer[15], 2 );
    }

    /// <summary>
    /// Get left slap gesture
    /// status value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    public void UpdateSlapLeftGesture( byte[] serialBuffer ) {
        this.slapLeft = IsBitSet( serialBuffer[16], 6 );
    }

    /// <summary>
    /// Get right slap gesture
    /// status value.
    /// </summary>
    /// <param name="serialBuffer">byte array - array data from the device</param>
    /// <returns>void</returns>
    public void UpdateSlapRightGesture( byte[] serialBuffer ) {
        this.slapRight = IsBitSet( serialBuffer[16], 7 );
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
    /// Init class method.
    /// </summary>
    /// <returns>void</returns>
    private void Init() {
        // set all attributes initial values.
        this.thumb = 0f;
        this.index = 0f;
        this.middle = 0f;
        this.ring = 0f;
        this.pinky = 0f;
        this.quaternions = new Quaternion( 0f, 0f, 0f, 0f );
        this.battery = 0f;
        this.ledButton = false;
        this.tap = false;
        this.trackPadCoordinates = new Vector2( 0f, 0f );
        this.accelerometer = new Vector3( 0f, 0f, 0f );
        this.cwScroll = false;
        this.acwScroll = false;

    }

}
