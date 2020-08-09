using System;

using Unity.Entities;

[Serializable]
public struct PrimitiveTag : ISharedComponentData
{
	public int IntVal;
	public byte ByteVal;
	public ulong ULongVal;
	public Enum EnumVal;
	public FlagEnum FlagEnumVal;

	public enum Enum
	{
		Val1,
		Val2,
		val3
	}

	[Flags]
	public enum FlagEnum
	{
		Val1 = 1 << 0,
		Val2 = 1 << 1,
		Val3 = 1 << 2,
		Val4 = 1 << 3,
		Val5 = Val2 | Val4,
	}
}