using System.Collections.Generic;
using System.Linq;

namespace SnhackRobotArm
{
	class RobotArm
	{
		const int SERVO_CENTER = 1500;

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
			BaseRotation.Position = SERVO_CENTER;
			BaseElevation.Position = SERVO_CENTER;
			LowerArmJoint.Position = SERVO_CENTER;
			UpperArmJoint.Position = SERVO_CENTER;
			WristRotation.Position = SERVO_CENTER;
			Gripper.Position = 2400;

			servos.ForEach(s => s.Speed = 2000);
			SendServoUpdateCommand(true);
			servos.ForEach(s => s.Speed = 500);
		}

		public void Park()
		{
			BaseRotation.Position = 600;
			BaseElevation.Position = 600;
			LowerArmJoint.Position = 600;
			UpperArmJoint.Position = 2400;
			WristRotation.Position = 1500;
			Gripper.Position = 1500;

			servos.ForEach(s => s.Speed = 2000);
			SendServoUpdateCommand(true);
			servos.ForEach(s => s.Speed = 500);
		}

		public void Move(VectorType vectorType, int amount)
		{
			var scaledAmount = amount / 50;
			var speed = 400 + amount / 10;

			switch (vectorType)
			{
				case VectorType.TranslateX:
					BaseRotation.Position += scaledAmount;
					BaseRotation.Speed = speed;
					break;
				case VectorType.TranslateY:
					BaseElevation.Position += scaledAmount;
					BaseElevation.Speed = speed;
					break;
				case VectorType.TranslateZ:
					LowerArmJoint.Position += scaledAmount;
					LowerArmJoint.Speed = speed;
					break;
				case VectorType.RotateX:
					UpperArmJoint.Position += scaledAmount;
					UpperArmJoint.Speed = speed;
					break;
				case VectorType.RotateY:
					WristRotation.Position += scaledAmount;
					WristRotation.Speed = speed;
					break;
				case VectorType.RotateZ:
					Gripper.Position += scaledAmount;
					Gripper.Speed = speed;
					break;
			}

			SendServoUpdateCommand(false);
		}

		private void SendServoUpdateCommand(bool force)
		{
			controllerBoard.SendServoUpdates(servos, force);
		}
	}
}
