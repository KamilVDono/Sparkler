namespace Primitives
{
	public interface IPrimitiveType
	{
		string GetFieldDeclaration( string fieldName, string accessModifier );
	}

	public struct Float : IPrimitiveType
	{
		private float _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} float {fieldName};";
	}

	public struct Double : IPrimitiveType
	{
		private double _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} double {fieldName};";
	}

	public struct Int : IPrimitiveType
	{
		private int _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} int {fieldName};";
	}

	public struct UInt : IPrimitiveType
	{
		private uint _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} uint {fieldName};";
	}

	public struct Byte : IPrimitiveType
	{
		private byte _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} byte {fieldName};";
	}

	public struct SByte : IPrimitiveType
	{
		private sbyte _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} sbyte {fieldName};";
	}

	public struct Long : IPrimitiveType
	{
		private long _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} long {fieldName};";
	}

	public struct ULong : IPrimitiveType
	{
		private ulong _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} ulong {fieldName};";
	}

	public struct Short : IPrimitiveType
	{
		private short _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} short {fieldName};";
	}

	public struct UShort : IPrimitiveType
	{
		private ushort _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} ushort {fieldName};";
	}

	public struct Decimal : IPrimitiveType
	{
		private decimal _backingField;

		public string GetFieldDeclaration( string fieldName, string accessModifier ) => $"\t\t{accessModifier} decimal {fieldName};";
	}
}