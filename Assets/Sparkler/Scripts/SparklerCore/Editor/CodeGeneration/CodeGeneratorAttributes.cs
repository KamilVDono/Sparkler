using System;

namespace Sparkler.Editor.CodeGeneration
{
	public class ProcessAfter : Attribute
	{
		public Type ReferenceType;

		public ProcessAfter( Type referenceType ) => ReferenceType = referenceType;
	}

	public class ProcessBefore : Attribute
	{
		public Type ReferenceType;

		public ProcessBefore( Type referenceType ) => ReferenceType = referenceType;
	}
}