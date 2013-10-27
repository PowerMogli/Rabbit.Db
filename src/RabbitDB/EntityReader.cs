using System;
using System.Collections.Generic;
using System.Data;
using RabbitDB.Mapping;
using RabbitDB.Materialization;
using RabbitDB.Storage;

namespace RabbitDB.Base
{
    public class EntityReader<TEntity> : IDisposable
    {
        private IDataReader _dataReader;
        private EntityMaterializer _materlizer;
        private DataReaderSchema _dataReaderSchema;
        private IDbProvider _dbProvider;
        private TableInfo _tableInfo;
        private bool _disposed;

        internal EntityReader(IDataReader dataReader, IDbProvider dbProvider)
        {
            _tableInfo = TableInfo<TEntity>.GetTableInfo;
            _dbProvider = dbProvider;
            _dataReader = dataReader;
            _materlizer = new EntityMaterializer(dbProvider);
            _dataReaderSchema = new DataReaderSchema(dataReader, _tableInfo);
        }

        /// <summary>
        /// Returns the current materialized entity object.
        /// </summary>
        public TEntity Current { get; private set; }

        /// <summary>
        /// Moves the reader to the next position in data stream.
        /// </summary>
        /// <returns>True if there have been any elements in the reader.</returns>
        public bool Read()
        {
            return Read(1);
        }

        internal bool Read(int step)
        {
            if (step < 0)
                throw new ArgumentException("Step is lower then 1. This is not allowed!", "step");

            for (int i = 0; i < step; i++)
            {
                if (_dataReader.Read() == false)
                {
                    Dispose();
                    return false;
                }
            }

            if (_tableInfo != null)
            {
                return ReadInternal();
            }

            return GetListOfPrimitivValues();
        }

        private bool GetListOfPrimitivValues()
        {
            this.Current = (TEntity)_dataReader.GetValue(0);
            return true;
        }

        private bool ReadInternal()
        {
            this.Current = _materlizer.Materialize<TEntity>(_dataReaderSchema, _dataReader);
            return true;
        }

        internal bool Load(TEntity entity)
        {
            if (_dataReader.Read() == false) return false;

            this.Current = _materlizer.Materialize<TEntity>(entity, _dataReaderSchema, _dataReader);
            return true;
        }

        internal void Load(TEntity entity, Action<TEntity, IDataReader> materializer)
        {
            materializer(entity, _dataReader);
        }

        internal IEnumerable<TEntity> Load(Func<IDataReader, IEnumerable<TEntity>> materializer)
        {
            return _materlizer.Materialize<TEntity>(materializer, _dataReader);
        }

        internal void Dispose()
        {
            if (_disposed
                || _dataReader == null
                || _dataReader.IsClosed) return;

            _dataReader.Close();
            _dataReader.Dispose();
            _dbProvider.Dispose();
            _disposed = true;
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
    }
}
