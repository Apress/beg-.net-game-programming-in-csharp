using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

/// <summary>
/// This class is where the DirectInput routines
/// for the application resides.
/// </summary>
public class InputClass {

	private const byte msgUp = 0;
	private const byte msgDown = 1;
	private const byte msgLeft = 2;
	private const byte msgRight = 3;
	private const byte msgCancelUp = 4;
	private const byte msgCancelDown = 5;
	private const byte msgCancelLeft = 6;
	private const byte msgCancelRight = 7;
	private bool pressedUp = false;
	private bool pressedDown = false;
	private bool pressedLeft = false;
	private bool pressedRight = false;

	private Control owner = null;
	private Device localDevice = null;

	public InputClass(Control owner) {
		this.owner = owner;

		localDevice = new Device(SystemGuid.Keyboard);
		localDevice.SetDataFormat(DeviceDataFormat.Keyboard);
		localDevice.SetCooperativeLevel(owner, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive);         
	}
    
	public Point GetInputState() {
		KeyboardState state = null;
		Point p = new Point(0);

		try {
			state = localDevice.GetCurrentKeyboardState();
		}
		catch(InputException) {
			do {
				Application.DoEvents();
				try{ localDevice.Acquire(); }
				catch (InputLostException) {
                  continue; }
				catch(OtherApplicationHasPriorityException) {
                  continue; }

				break;

			}while( true );
		}

		if(null == state)
			return p;

		if(state[Key.Down]) {
			pressedDown = true;
		}
		else if (pressedDown == true) {
			pressedDown = false;
		}
		if(state[Key.Up]) {
			pressedUp = true;
		}
		else if (pressedUp == true) {
			pressedUp = false;
		}
		if(state[Key.Left]) {
			pressedLeft = true;
		}
		else if (pressedLeft == true) {
			pressedLeft = false;
		}
		if(state[Key.Right]) {
			pressedRight  = true;
		}
		else if (pressedRight == true) {
			pressedRight = false;
		}

		return p;
	}
}