using System.Collections.Generic;

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
			WristRotation = new Servo(5);
			Gripper = new Servo(6);
			servos = new List<Servo> { BaseRotation, BaseElevation, LowerArmJoint, UpperArmJoint, WristRotation, Gripper };
		}

		public void Reset()
		{
			BaseRotation.Position = SERVO_CENTER;
			BaseElevation.Position = SERVO_CENTER;
			LowerArmJoint.Position = SERVO_CENTER;
			UpperArmJoint.Position = SERVO_CENTER;
			WristRotation.Position = SERVO_CENTER;
			Gripper.Position = SERVO_CENTER;

			SendServoUpdateCommand();
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

			SendServoUpdateCommand();
		}

		private void SendServoUpdateCommand()
		{
			controllerBoard.SendServoUpdates(servos);
		}
	}
}
