namespace Sparkler.Example.Shooter.Components
{
	public enum ButtonState : byte
	{
		Incative = 1,
		StartPressing = 2,
		Hold = 3,
	}

	public static class ButtonStateExt
	{
		public static ButtonState Update( this ButtonState previousState, bool isPressed )
		{
			if ( isPressed )
			{
				if ( previousState == ButtonState.Incative )
				{
					return ButtonState.StartPressing;
				}
				if ( previousState == ButtonState.StartPressing )
				{
					return ButtonState.Hold;
				}
				return previousState;
			}
			else
			{
				if ( previousState == ButtonState.StartPressing )
				{
					return ButtonState.Incative;
				}
				if ( previousState == ButtonState.Hold )
				{
					return ButtonState.Incative;
				}
				return previousState;
			}
		}
	}
}