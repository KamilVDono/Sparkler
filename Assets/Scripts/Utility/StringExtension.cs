namespace FSM.Utility
{
	public static class StringExtension
	{
		public static string ToUpperFirstChar( this string value ) => char.ToUpperInvariant( value[0] ) + value.Substring( 1 );

		public static string ToLowerFirstChar( this string value ) => char.ToLowerInvariant( value[0] ) + value.Substring( 1 );
	}
}