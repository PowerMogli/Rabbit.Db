using System;
using System.Collections.Generic;
using System.Data;
//using LinFu.DynamicProxy;
using RabbitDB.Mapping;
using RabbitDB.Storage;

namespace RabbitDB.Materialization
{
    class EntityMaterializer : IEntityMaterializer
    {
        //private ProxyFactory _proxyFactory = new ProxyFactory();
        private IDbProvider _dbProvider;

        internal EntityMaterializer(IDbProvider provider)
        {
            _dbProvider = provider;
        }

        public IEnumerable<TEntity> Materialize<TEntity>(Func<IDataReader, IEnumerable<TEntity>> materializer, IDataReader dataReader)
        {
            return materializer(dataReader);
        }

        public TEntity Materialize<TEntity>(TEntity entity, IDataSchemaCreator dataReaderSchema, IDataRecord dataRecord)
        {
            TableInfo tableInfo = TableInfo<TEntity>.GetTableInfo;

            for (int index = 0; index < tableInfo.Columns.Count; index++)
            {
                IPropertyInfo propertyInfo = tableInfo.Columns[index];
                int columnIndex = dataReaderSchema.ColumnIndex(index);
                if (columnIndex < 0) continue;

                MaterializeEntity(entity, propertyInfo, dataRecord[columnIndex]);
            }

            return entity;
        }

        public TEntity Materialize<TEntity>(IDataSchemaCreator dataReaderSchema, IDataRecord dataRecord)
        {
            TEntity entity = Activator.CreateInstance<TEntity>();
            return Materialize<TEntity>(entity, dataReaderSchema, dataRecord);
        }

        private void MaterializeEntity(object entity, IPropertyInfo propertyInfo, object value)
        {
            if (!propertyInfo.CanWrite) return;

            if (Convert.IsDBNull(value))
                value = propertyInfo.IsNullable ? null : _dbProvider.ResolveNullValue(value, propertyInfo.PropertyType);

            propertyInfo.SetValue(entity, value);
        }
    }
}