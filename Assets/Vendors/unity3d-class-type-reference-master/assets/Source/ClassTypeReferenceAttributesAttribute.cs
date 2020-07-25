using System;
using System.Collections.Generic;
using System.Linq;

namespace Rotorz.Games
{
	[AttributeUsage( AttributeTargets.Field, AllowMultiple = false )]
	public class ClassTypeReferenceAttributesAttribute : ClassTypeConstraintAttribute
	{
		private Type[] _containedTypes = new Type[0];
		private ClassTypeConstraintAttribute[] _container;

		private IEnumerable<ClassTypeConstraintAttribute> Container => _container ?? ( _container = _containedTypes
			.Select( t => Activator.CreateInstance( t ) )
			.OfType<ClassTypeConstraintAttribute>().ToArray() );

		public ClassTypeReferenceAttributesAttribute( params Type[] containedTypes ) => _containedTypes = containedTypes;

		public override bool IsConstraintSatisfied( Type type ) => Container.All( constrain => constrain.IsConstraintSatisfied( type ) );
	}
}