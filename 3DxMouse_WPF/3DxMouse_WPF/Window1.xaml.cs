using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Interop;
using Application = System.Windows.Application;
using System.IO.Ports;

namespace _3DxMouse_WPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        _3DxMouse._3DxMouse  my3DxMouse;
        Quaternion currentOrientation = new Quaternion();

        Message message = new Message();

		SerialPort port;

        public Window1()
        {
            Activate();

			try
			{
				port = new SerialPort("COM22", 115200, Parity.None, 8, StopBits.One);
				port.Open();
			}
			catch (Exception)
			{
			}
        }

		double[] servos = new double[] { 1500, 1500, 1500, 1500, 1500, 1500 };
		int servoSpeed = 500;
		double SERVO_MIN = 600;
		double SERVO_MAX = 2400;

        private void MotionEvent(object sender, _3DxMouse._3DxMouse.MotionEventArgs e)
        {
            Vector3D tv = new Vector3D();
            Vector3D rv = new Vector3D();

            // Change the device?
            if ((string)this.DevicesComboBox.SelectedValue != e.DeviceInfo.deviceName)
                this.DevicesComboBox.SelectedValue = e.DeviceInfo.deviceName;

            // Currently Translations and Rotations arrive separately (T first, then R)
            // but there is a firmware version that will deliver both together

            // Translation Vector?
            if (e.TranslationVector != null)
            {
				e.TranslationVector.X /= 10;
				e.TranslationVector.Y /= 10;
				e.TranslationVector.Z /= 10;

                // Swap axes from HID orientation to a right handed coordinate system that matches WPF model space
				if (Math.Abs(e.TranslationVector.X) > Math.Abs(e.TranslationVector.Y) 
					&& Math.Abs(e.TranslationVector.X) > Math.Abs(e.TranslationVector.Z))
					tv.X =  e.TranslationVector.X;
				if (Math.Abs(e.TranslationVector.Z) > Math.Abs(e.TranslationVector.X) 
					&& Math.Abs(e.TranslationVector.Z) > Math.Abs(e.TranslationVector.Y))
					tv.Y = -e.TranslationVector.Z;
				if (Math.Abs(e.TranslationVector.Y) > Math.Abs(e.TranslationVector.X) 
					&& Math.Abs(e.TranslationVector.Y) > Math.Abs(e.TranslationVector.Z))
					tv.Z =  e.TranslationVector.Y;
            }

            // Rotation Vector?
            if (e.RotationVector != null)
			{
				e.RotationVector.X /= 10;
				e.RotationVector.Y /= 10;
				e.RotationVector.Z /= 10;

                // Swap axes from HID orientation to a right handed coordinate system that matches WPF model space
				if (Math.Abs(e.RotationVector.X) > Math.Abs(e.RotationVector.Y) 
					&& Math.Abs(e.RotationVector.X) > Math.Abs(e.RotationVector.Z))
					rv.X =  e.RotationVector.X;
				if (Math.Abs(e.RotationVector.Z) > Math.Abs(e.RotationVector.X) 
					&& Math.Abs(e.RotationVector.Z) > Math.Abs(e.RotationVector.Y))
					rv.Y = -e.RotationVector.Z;
				if (Math.Abs(e.RotationVector.Y) > Math.Abs(e.RotationVector.X) 
					&& Math.Abs(e.RotationVector.Y) > Math.Abs(e.RotationVector.Z))
					rv.Z =  e.RotationVector.Y;
            }
			
			if (Math.Abs(tv.X) + Math.Abs(tv.Y) + Math.Abs(tv.Z) >
				Math.Abs(rv.X) + Math.Abs(rv.Y) + Math.Abs(rv.Z))
			{
				rv.X = 0;
				rv.Y = 0;
				rv.Z = 0;
			}
			else
			{
				tv.X = 0;
				tv.Y = 0;
				tv.Z = 0;
			}
			
			this.XDataLabel.Content = tv.X.ToString();
			this.YDataLabel.Content = tv.Y.ToString();
			this.ZDataLabel.Content = tv.Z.ToString();

			this.RxDataLabel.Content = rv.X.ToString();
			this.RyDataLabel.Content = rv.Y.ToString();
			this.RzDataLabel.Content = rv.Z.ToString();


			servos[0] += tv.X / 5;
			servos[1] += tv.Y / 5;
			servos[2] += tv.Z / 5;

			servos[3] += rv.X / 5;
			servos[4] += rv.Y / 5;
			servos[5] += rv.Z / 5;


			for (int i = 0; i < 6; i++)
			{
				if (servos[i] < SERVO_MIN) servos[i] = SERVO_MIN;
				if (servos[i] > SERVO_MAX) servos[i] = SERVO_MAX;
			}

			servoSpeed = 400 + (int)tv.Length * 2;

			Console.WriteLine(tv.Length);
			lServo0.Content = servos[0];
			lServo1.Content = servos[1];
			lServo2.Content = servos[2];
			lServo3.Content = servos[3];
			lServo4.Content = servos[4];
			lServo5.Content = servos[5];


			UpdateServos();
            //UpdateCube(tv, rv);
        }

		DateTime lastServoCommand = DateTime.Now;

		private void UpdateServos()
		{
			if ((DateTime.Now - lastServoCommand).TotalMilliseconds > 100)
			{
				for (int i = 0; i < 6; i++)
				{
					var cmd = string.Format("# {0} P {1} S {2}", GetMappedServerNo(i), servos[i], servoSpeed);
					Console.Write(cmd);
					if (port.IsOpen)
						port.Write(cmd);
				}

				Console.WriteLine();
				if (port.IsOpen)
					port.Write("\r");

				lastServoCommand = DateTime.Now;
			}
		}

		private int GetMappedServerNo(int i)
		{
			switch (i)
			{
				case 4: return 5;
				case 1: return 6;
				case 2: return 4;
				case 3: return 3;
				case 0: return 2;
				case 5: return 1;
			}

			return 0;
		}

        private void ButtonEvent(object sender, _3DxMouse._3DxMouse.ButtonEventArgs e)
        {
            // Change the device?
            if ((string)this.DevicesComboBox.SelectedValue != e.DeviceInfo.deviceName)
                this.DevicesComboBox.SelectedValue = e.DeviceInfo.deviceName;

            // Show the buttons that are pressed
            this.ButtonsLabel.Content = e.ButtonMask.Pressed.ToString("X");

            ResetCube();
			ResetServos();
			UpdateServos();
        }

		private void ResetServos()
		{
			for (int i = 0; i < 6; i++)
				servos[i] = 1500;
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

            // Populate the cube faces
            InitCube();
        }

        void CloseMe(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void InitCube()
        {
            // Add jpgs to the 1 side
            System.IO.DirectoryInfo W7VideoDir = new System.IO.DirectoryInfo("c:\\users\\public\\videos\\Sample Videos");
            // Add jpgs to the 5-6 sides
            System.IO.DirectoryInfo W7PictureDir = new System.IO.DirectoryInfo("c:\\users\\public\\Pictures\\Sample Pictures");
            System.IO.DirectoryInfo VistaPictureDir = new System.IO.DirectoryInfo("c:\\users\\public\\Public Pictures\\Sample Pictures");
            String pictureDir;
            if (W7PictureDir.Exists)
                pictureDir = W7PictureDir.FullName;
            else if (VistaPictureDir.Exists)
                pictureDir = VistaPictureDir.FullName;
            else
                pictureDir = ".";

            int faceIndex = 0;
            //
            // This doesn't work to map a video to a face.
            //if (W7VideoDir.Exists)
            //{
            //    foreach (string fileName in System.IO.Directory.GetFiles(W7VideoDir.FullName, "*.wmv"))
            //    {
            //        // Map the sample video to the first face
            //        GeometryModel3D face = this.Cube.Children[faceIndex] as GeometryModel3D;
            //        DiffuseMaterial diffuseMaterial = face.Material as DiffuseMaterial;
            //        MediaPlayer mp = new MediaPlayer();
            //        mp.Open(new Uri(fileName, UriKind.RelativeOrAbsolute));
            //        VideoDrawing vd = new VideoDrawing();
            //        vd.Player = mp;
            //        DrawingBrush db = new DrawingBrush();
            //        db.Drawing = vd;
            //        diffuseMaterial.Brush = db;
            //        vd.Player.Play();
            //        if (++faceIndex >= 6)
            //            break;
            //    }
            //}

            if (faceIndex < 6)
            {
                foreach (string fileName in System.IO.Directory.GetFiles(pictureDir, "*.jpg"))
                {
                    // Console.WriteLine(fileName);
                    GeometryModel3D face = this.Cube.Children[faceIndex] as GeometryModel3D;
                    DiffuseMaterial diffuseMaterial = face.Material as DiffuseMaterial;
                    diffuseMaterial.Brush = new ImageBrush(new BitmapImage(new Uri(fileName, UriKind.RelativeOrAbsolute)));

                    // Only 6 faces on a cube
                    if (++faceIndex >= 6)
                        break;
                }
            }

        }

        public void UpdateCube(Vector3D tv, Vector3D rv)
        {
            ModelVisual3D mv = this.theViewport3D.Children[1] as ModelVisual3D;
            Transform3DGroup t3dg = mv.Transform as Transform3DGroup;

            //ScaleTransform3D _GroupScaleTransform = t3dg.Children[0] as ScaleTransform3D;  (no scaling, it's a perspective view)
            RotateTransform3D _GroupRotateTransform = t3dg.Children[1] as RotateTransform3D;
            TranslateTransform3D _GroupTranslateTransform = t3dg.Children[2] as TranslateTransform3D;


            //_GroupScaleTransform.ScaleX = 1.0;
            //_GroupScaleTransform.ScaleY = 1.0;
            //_GroupScaleTransform.ScaleZ = 1.0;

            if (rv != null)
            {
                Vector3D axisOfRotation = new Vector3D(rv.X, rv.Y, rv.Z);
                double amountOfRotation = axisOfRotation.Length;
                if (amountOfRotation > 0)
                {
                    axisOfRotation.Normalize();
                    currentOrientation = new Quaternion(axisOfRotation, amountOfRotation / 20.0) * currentOrientation;
                    _GroupRotateTransform.Rotation = new QuaternionRotation3D(currentOrientation);
                }
            }

            if (tv != null)
            {
                _GroupTranslateTransform.OffsetX += (double)tv.X / 1000.0;
                _GroupTranslateTransform.OffsetY += (double)tv.Y / 1000.0;
                _GroupTranslateTransform.OffsetZ += (double)tv.Z / 1000.0;
            }

        }

        public void ResetCube()
        {
            ModelVisual3D mv = this.theViewport3D.Children[1] as ModelVisual3D;
            Transform3DGroup t3dg = mv.Transform as Transform3DGroup;

            RotateTransform3D _GroupRotateTransform = t3dg.Children[1] as RotateTransform3D;
            TranslateTransform3D _GroupTranslateTransform = t3dg.Children[2] as TranslateTransform3D;

            _GroupRotateTransform.Rotation = new QuaternionRotation3D(currentOrientation = new Quaternion());

            _GroupTranslateTransform.OffsetX = 
            _GroupTranslateTransform.OffsetY =
            _GroupTranslateTransform.OffsetZ = 0.0;
        }
    }
}
