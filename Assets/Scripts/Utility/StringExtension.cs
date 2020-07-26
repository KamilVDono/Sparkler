namespace FSM.Utility
{
	public static class StringExtension
	{
		public static string ToUpperFirstChar( this string value ) => char.ToUpperInvariant( value[0] ) + value.Substring( 1 );
	}
}