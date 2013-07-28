using System;

namespace _3DxMouse_WPF
{
	class ProcessedMotionEventArgs : EventArgs
	{
		public VectorType Vector { get; private set; }
		public int Amount { get; private set; }

		public ProcessedMotionEventArgs(VectorType vector, int amount)
		{
			Vector = vector;
			Amount = amount;
		}
	}

	enum VectorType
	{
		TranslateX,
		TranslateY,
		TranslateZ,
		RotateX,
		RotateY,
		RotateZ
	}
}
