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
			Gripper = new Servo(6, 1600, 2200, 500);
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
            
            CalcHeight();

            if (Height > 2)
            {
                SendServoUpdateCommand(false);
            }
            else
            {
                switch (vectorType)
                {
                    case VectorType.TranslateX:
                        Gripper.Position -= scaledAmount;
                        break;
                    case VectorType.TranslateY:
                        BaseElevation.Position -= scaledAmount;
                        break;
                    case VectorType.TranslateZ:
                        UpperArmJoint.Position -= scaledAmount;
                        break;
                    case VectorType.RotateX:
                        LowerArmJoint.Position -= scaledAmount;
                        break;
                    case VectorType.RotateY:
                        WristRotation.Position -= scaledAmount;
                        break;
                    case VectorType.RotateZ:
                        BaseRotation.Position -= scaledAmount;
                        break;
                }
            }
		}

		private void SetServoSpeedToMax()
		{
			servos.ForEach(s => s.Speed = Servo.SPEED_HIGH);
		}

		private void SendServoUpdateCommand(bool force)
		{
			controllerBoard.SendServoUpdates(servos, force);
		}

        private void CalcHeight()
        {
            double Base = BaseElevation.PositionDegrees;
            double Lower = LowerArmJoint.PositionDegrees;
            double Upper = UpperArmJoint.PositionDegrees;

            const int BASE_HEIGHT = 9;   //Height of base
            const int BASE_LENGTH = 8;   //Length of Base arm
            const int LOWER_LENGTH = 8;  //Length of lower arm
            const int UPPER_LENGTH = 20; //6cm arm + 14 cm gripper length

            //Initialize Height as Base height
            double height = BASE_HEIGHT;

            //Base arm
            double BaseAngle = 180 - Base;
            double x = BASE_LENGTH * Math.Sin((BaseAngle * Math.PI) / 180);
            height += x;

            //Mid arm / Lower Joint
            double HorzAngle = 90 - BaseAngle;
            double LowerAngle = Lower - HorzAngle;
            x = LOWER_LENGTH * Math.Sin((LowerAngle * Math.PI) / 180);

            /* If Below Horizontal/subtract Height
             * If Above Horizontal/add to Height*/
            if (LowerArmJoint.PositionDegrees < HorzAngle)
            { 
                height -= x;
            }
            else
            {
                height += x;
            }

            //Top arm / Upper Joint
            HorzAngle = LowerAngle - BaseAngle;
            double UpperAngle = Upper - HorzAngle;
            x = UPPER_LENGTH * Math.Sin((UpperAngle * Math.PI) / 180);

            /* If Below Horizontal/subtract Height
             * If Above Horizontal/add to Height*/
            if (Upper < HorzAngle)
            {
                height -= x;
            }
            else
            {
                height += x;
            }
            //return Math.Round(Height, 3);  
            Height = Math.Round(height, 3);  
        }

        public double Height
        {
            get;
            private set;
        }
	}
}
