using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace SnhackRobotArm
{
    public partial class Window1 : Window
    {
        _3DxMouse._3DxMouse  my3DxMouse;
		Message message = new Message();
		MouseEventProcessor mouseEventProcessor = new MouseEventProcessor();
        RobotArm arm = new RobotArm();

        public Window1()
        {
            Activate();
			mouseEventProcessor.ProcessedMotion += new EventHandler<ProcessedMotionEventArgs>(mouseEventProcessor_ProcessedMotion);
        }

		private void ButtonEvent(object sender, _3DxMouse._3DxMouse.ButtonEventArgs e)
		{
			CheckDeviceChange(e.DeviceInfo);

			// Show the buttons that are pressed
			this.ButtonsLabel.Content = e.ButtonMask.Pressed.ToString("X");

			arm.Reset();
		}
		
		private void MotionEvent(object sender, _3DxMouse._3DxMouse.MotionEventArgs e)
        {
			CheckDeviceChange(e.DeviceInfo);

			// Show the vectors that are being genenerated by the controller
			if (e.TranslationVector != null)
			{
				this.XDataLabel.Content = e.TranslationVector.X.ToString();
				this.YDataLabel.Content = e.TranslationVector.Y.ToString();
				this.ZDataLabel.Content = e.TranslationVector.Z.ToString();
			}
			if (e.RotationVector != null)
			{
				this.RxDataLabel.Content = e.RotationVector.X.ToString();
				this.RyDataLabel.Content = e.RotationVector.Y.ToString();
				this.RzDataLabel.Content = e.RotationVector.Z.ToString();
			}

			mouseEventProcessor.Process(e);
        }

		void mouseEventProcessor_ProcessedMotion(object sender, ProcessedMotionEventArgs e)
		{
			arm.Move(e.Vector, e.Amount);

			lServo0.Content = arm.BaseRotation.Position;
			lServo1.Content = arm.BaseElevation.Position;
			lServo2.Content = arm.LowerArmJoint.Position;
			lServo3.Content = arm.UpperArmJoint.Position;
			lServo4.Content = arm.WristRotation.Position;
			lServo5.Content = arm.Gripper.Position;
		}

		private void CheckDeviceChange(_3DxMouse._3DxMouse.DeviceInfo deviceInfo)
		{
			if ((string)this.DevicesComboBox.SelectedValue != deviceInfo.deviceName)
				this.DevicesComboBox.SelectedValue = deviceInfo.deviceName;
		}

		public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (my3DxMouse != null)
            {
                // I could have done one of two things here.
                // 1. Use a Message as it was used before.
                // 2. Changes the ProcessMessage method to handle all of these parameters(more work).
                //    I opted for the easy way.

                //Note: Depending on your application you may or may not want to set the handled param.

                message.HWnd = hwnd;
                message.Msg = msg;
                message.LParam = lParam;
                message.WParam = wParam;

                my3DxMouse.ProcessMessage(message);
            }
            return IntPtr.Zero;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // I am new to WPF and I don't know where else to call this function.
            // It has to be called after the window is created or the handle won't
            // exist yet and the function will throw an exception.
            StartWndProcHandler();

            base.OnSourceInitialized(e);
        }

        void StartWndProcHandler()
        {
            IntPtr hwnd = IntPtr.Zero;
            Window myWin = Application.Current.MainWindow;

            try
            {
                hwnd = new WindowInteropHelper(myWin).Handle;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //Get the Hwnd source   
            HwndSource source = HwndSource.FromHwnd(hwnd);
            //Win32 queue sink
            source.AddHook(new HwndSourceHook(WndProc));

            // Connect to Raw Input & find devices
            my3DxMouse = new _3DxMouse._3DxMouse(source.Handle);
            int NumberOf3DxMice = my3DxMouse.EnumerateDevices();

            // Setup event handlers to be called when something happens
            my3DxMouse.MotionEvent += new _3DxMouse._3DxMouse.MotionEventHandler(MotionEvent);
            my3DxMouse.ButtonEvent += new _3DxMouse._3DxMouse.ButtonEventHandler(ButtonEvent);

            // Add devices to device list comboBox
            foreach (System.Collections.DictionaryEntry listEntry in my3DxMouse.deviceList)
            {
                _3DxMouse._3DxMouse.DeviceInfo devInfo = (_3DxMouse._3DxMouse.DeviceInfo)listEntry.Value;
                this.DevicesComboBox.Items.Add(devInfo.deviceName);
            }
            if (my3DxMouse.deviceList.Count > 0)
            {
                this.DevicesComboBox.SelectedIndex = 0;
            }
        }
	}
}
