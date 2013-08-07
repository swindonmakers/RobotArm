using System;
using System.Collections.Generic;

namespace SnhackRobotArm
{
	class RobotArm
	{
		List<Servo> servos;
		public Servo BaseRotation { get; private set; }
		public Servo BaseElevation { get; private set; }
		public Servo LowerArmJoint { get; private set; }
		public Servo UpperArmJoint { get; private set; }
		public Servo WristRotation { get; private set; }
		public Servo Gripper { get; private set; }
		
		ServoControllerBoard controllerBoard;

		public RobotArm()
		{
			controllerBoard = new ServoControllerBoard();
			BaseRotation = new Servo(1);
			BaseElevation = new Servo(2); 
			LowerArmJoint = new Servo(3);
			UpperArmJoint = new Servo(4);
			WristRotation = new Servo(5, 600, 2200, 500);
			Gripper = new Servo(6, 650, 2200, 500);
			servos = new List<Servo> { BaseRotation, BaseElevation, LowerArmJoint, UpperArmJoint, WristRotation, Gripper };
		}

		public void Reset()
		{
			BaseRotation.Position = Servo.CENTER_POSITION;
			BaseElevation.Position = Servo.CENTER_POSITION;
			LowerArmJoint.Position = Servo.CENTER_POSITION;
			UpperArmJoint.Position = Servo.CENTER_POSITION;
			WristRotation.Position = Servo.CENTER_POSITION;
			Gripper.Position = 2400;

			SetServoSpeedToMax();
			SendServoUpdateCommand(true);
		}

		public void Park()
		{
			BaseRotation.Position = 600;
			BaseElevation.Position = 600;
			LowerArmJoint.Position = 600;
			UpperArmJoint.Position = 2400;
			WristRotation.Position = 1500;
			Gripper.Position = 1500;

			SetServoSpeedToMax();
			SendServoUpdateCommand(true);
		}

		public void Move(VectorType vectorType, int amount)
		{
			var scaledAmount = amount / 50;
			var speed = 500 + 2 * Math.Abs(amount);

			switch (vectorType)
			{
				case VectorType.TranslateX:
					Gripper.Position += scaledAmount;
					Gripper.Speed = speed;
					break;
				case VectorType.TranslateY:
					BaseElevation.Position += scaledAmount;
					BaseElevation.Speed = speed;
					break;
				case VectorType.TranslateZ:
					UpperArmJoint.Position += scaledAmount;
					UpperArmJoint.Speed = speed;
					break;
				case VectorType.RotateX:
					LowerArmJoint.Position += scaledAmount;
					LowerArmJoint.Speed = speed;
					break;
				case VectorType.RotateY:
					WristRotation.Position += scaledAmount;
					WristRotation.Speed = speed;
					break;
				case VectorType.RotateZ:
					BaseRotation.Position += scaledAmount;
					BaseRotation.Speed = speed;
					break;
			}

			SendServoUpdateCommand(false);
		}

		private void SetServoSpeedToMax()
		{
			servos.ForEach(s => s.Speed = Servo.SPEED_HIGH);
		}

		private void SendServoUpdateCommand(bool force)
		{
			controllerBoard.SendServoUpdates(servos, force);
		}
	}
}
