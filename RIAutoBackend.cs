using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace RelationsInspector
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public class RelatedAttribute : Attribute { }

    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public class RelatingAttribute : Attribute { }
}

namespace RelationsInspector.Backend
{
    public class RIAutoBackend<T> : MinimalBackend<T,string> where T : class
    {        
        IEnumerable<FieldInfo> relatedFields;
        IEnumerable<FieldInfo> relatingFields;

        public override IEnumerable<T> Init( IEnumerable<object> targets, RelationsInspectorAPI api )
        {
            relatingFields = ReflectionUtil.GetAttributeFields<T, RelatingAttribute>( );
            relatedFields = ReflectionUtil.GetAttributeFields<T, RelatedAttribute>( );

            return base.Init( targets, api );
        }

        public override IEnumerable<Relation<T, string>> GetRelations( T entity )
        {
            var outRelations = relatedFields
                .SelectMany( fInfo => ReflectionUtil.GetValues<T>( fInfo, entity ) )
                .Select( other => new Relation<T, string>( entity, other, string.Empty ) );

            var inRelations = relatingFields
                .SelectMany( fInfo => ReflectionUtil.GetValues<T>( fInfo, entity ) )
                .Select( other => new Relation<T, string>( other, entity, string.Empty ) );

            return outRelations.Concat( inRelations );
        }
    }
}
