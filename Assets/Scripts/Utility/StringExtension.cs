using System;
using System.Text;

namespace FSM.Utility
{
	/// <summary>
	/// Use this struct for faster and with minimal allocations concatenations
	/// </summary>
	/// <example>
	/// var output = S.Concat() + str1 + ... + strN + valueType1 + ... + valueTypeN + obejct1 + ... + objectN;
	/// </example>
	public struct S
	{
		private static StringBuilder s_stringBuilder = new StringBuilder(1024);

		public static S Concat()
		{
			s_stringBuilder.Clear();
			return new S();
		}

		public static S Concat( string startingString )
		{
			s_stringBuilder.Clear();
			return new S() + startingString;
		}

		public static S Concat( char startingString )
		{
			s_stringBuilder.Clear();
			return new S() + startingString;
		}

		public static implicit operator string( S _ ) => s_stringBuilder.ToString();

		public override string ToString() => s_stringBuilder.ToString();

		#region + operator

		public static S operator +( S left, char right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, string right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, bool right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, ulong right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, uint right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, byte right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, float right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, ushort right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, object right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, char[] right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, sbyte right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, decimal right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, short right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, int right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, long right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, double right )
		{
			s_stringBuilder.Append( right );
			return left;
		}

		public static S operator +( S left, DateTime right )
		{
			s_stringBuilder.Append( right.ToString() );
			return left;
		}

		public static S operator +( S left, TimeSpan right )
		{
			s_stringBuilder.Append( right.ToString() );
			return left;
		}

		#endregion + operator
	}

	public static class StringExtension
	{
		public static string ToUpperFirstChar( this string value ) => char.ToUpperInvariant( value[0] ) + value.Substring( 1 );

		public static string ToLowerFirstChar( this string value ) => char.ToLowerInvariant( value[0] ) + value.Substring( 1 );
	}
}