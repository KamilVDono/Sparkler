namespace FSM.Primitives
{
	public interface IPrimitiveType
	{
		string GetFieldDeclaration( string fieldName, string accessModifier );
	}

	public struct Float : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} float {fieldName};";
	}

	public struct Double : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} double {fieldName};";
	}

	public struct Int : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} int {fieldName};";
	}

	public struct UInt : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} uint {fieldName};";
	}

	public struct Char : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} char {fieldName};";
	}

	public struct Byte : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} byte {fieldName};";
	}

	public struct SByte : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} sbyte {fieldName};";
	}

	public struct Long : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} long {fieldName};";
	}

	public struct ULong : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} ulong {fieldName};";
	}

	public struct Short : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} short {fieldName};";
	}

	public struct UShort : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} ushort {fieldName};";
	}

	public struct Decimal : IPrimitiveType
	{
		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} decimal {fieldName};";
	}
}