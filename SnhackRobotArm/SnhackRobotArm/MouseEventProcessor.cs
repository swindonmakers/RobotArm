using System;
using System.Collections.Generic;
using System.Linq;

namespace SnhackRobotArm
{
	class MouseEventProcessor
	{
		_3DxMouse._3DxMouse.TranslationVector translationVector;
		_3DxMouse._3DxMouse.RotationVector rotationVector;

		public event EventHandler<ProcessedMotionEventArgs> ProcessedMotion;

		public void Process(_3DxMouse._3DxMouse.MotionEventArgs e)
		{
			if (e.TranslationVector != null)
				translationVector = e.TranslationVector;
			if (e.RotationVector != null)
				rotationVector = e.RotationVector;

			if (e.TranslationVector != null && e.RotationVector != null)
			{
				Dictionary<VectorType, int> vectors = new Dictionary<VectorType, int>();
				vectors.Add(VectorType.TranslateX, translationVector.X);
				vectors.Add(VectorType.TranslateY, translationVector.Y);
				vectors.Add(VectorType.TranslateZ, translationVector.Z);
				vectors.Add(VectorType.RotateX, rotationVector.X);
				vectors.Add(VectorType.RotateY, rotationVector.Y);
				vectors.Add(VectorType.RotateZ, rotationVector.Z);

				e.TranslationVector = null;
				e.RotationVector = null;

				var maxVector = vectors.Max();

				var handler = ProcessedMotion;
				if (handler != null)
					handler(this, new ProcessedMotionEventArgs(maxVector.Key, maxVector.Value));
			}
		}
	}
}
