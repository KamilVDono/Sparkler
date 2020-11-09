using System;

using Unity.Entities;

[Serializable]
public struct TestTag : ISharedComponentData
{
	// Add fields to your component here. Remember that:
	//
	// * A component itself is for storing data and doesn't 'do' anything.
	//
	// * To act on the data, you will need a System.
	//
	// * Data in a component must be blittable, which means a component can
	//   only contain fields which are primitive types or other blittable
	//   structs; they cannot contain references to classes.
	//
	// * You should focus on the data structure that makes the most sense
	//   for runtime use here. Authoring Components will be used for
	//   authoring the data in the Editor.

	public int IntValue;
	public SomeEnum Enum;
	public FlagEnum FlagEnumValue;
	public byte Byte;
	public ComposedStruct ComposedStructValue;

	public enum SomeEnum
	{
		ValueA,
		ValueB,
	}

	[Flags]
	public enum FlagEnum
	{
		ValueA = 1 << 0,
		ValueB = 1 << 1,
		ValueC = 1 << 2,
	}

	[Serializable]
	public struct ComposedStruct
	{
		public int Val1;
		public float Val2;
		public ulong Val3;
	}
}