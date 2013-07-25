using System;

namespace _3DxMouse_WPF
{
	class Servo
	{
		public Servo(int number) : this(number, 600, 2400, 500) { }

		public Servo(int number, int minimum, int maximum, int speed)
		{
			Number = number;
			Minimum = minimum;
			Maximum = maximum;
			Speed = speed;
		}

		public int Number { get; private set; }
		public int Minimum { get; private set; }
		public int Maximum { get; private set; }
		public int Speed { get; set; }

		private double position;
		public double Position
		{
			get { return position; }
			set
			{
				if (value < Minimum)
					position = Minimum;
				else if (value > Maximum)
					position = Maximum;
				else
					position = value;
			}
		}

		public string UpdateCommand()
		{
			return string.Format("# {0} P {1} S {2}", GetMappedServerNo(Number), Position, Speed);
		}

		/// <summary>
		/// Returns the number we need to send to the control board based on the label on the 
		/// port on the board
		/// </summary>
		private int GetMappedServerNo(int servoNumber)
		{
			switch (servoNumber)
			{
				case 5: return 5;
				case 2: return 6;
				case 3: return 4;
				case 4: return 3;
				case 1: return 2;
				case 6: return 1;
				default: throw new ArgumentOutOfRangeException("servoNumber", servoNumber, "Can't map servo number");
			}
		}
	}
}