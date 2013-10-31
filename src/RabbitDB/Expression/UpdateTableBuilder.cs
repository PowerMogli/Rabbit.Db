using System;
using System.Linq.Expressions;
using RabbitDB.Mapping;
using RabbitDB.Storage;

namespace RabbitDB.Expressions
{
    public interface IBuildUpdateTable<T>
    {
        IBuildUpdateTable<T> Set(Expression<Func<T, object>> column, Expression<Func<T, object>> statement);
        IBuildUpdateTable<T> Set(Expression<Func<T, object>> column, object value);
        void Set(string column, object value);
        void Where(Expression<Func<T, bool>> criteria);
    }

    internal class UpdateTableBuilder<T> : IBuildUpdateTable<T>
    {
        private readonly SqlExpressionBuilder<T> _builder;
        private TableInfo _tableInfo;
        private IDbProvider _dbProvider;

        public UpdateTableBuilder(IDbProvider dbProvider)
        {
            _builder = new SqlExpressionBuilder<T>(dbProvider);
            _dbProvider = dbProvider;
            _tableInfo = TableInfo<T>.GetTableInfo;
            _builder.Append(_tableInfo.GetBaseUpdate(dbProvider));
        }

        public IBuildUpdateTable<T> Set(Expression<Func<T, object>> column, Expression<Func<T, object>> statement)
        {
            _builder.Append(string.Format(" {0}=", _dbProvider.EscapeName(_tableInfo.ResolveColumnName(column.Body.GetPropertyName()))));
            _builder.Write(statement);
            _builder.Append(",");
            return this;
        }

        public IBuildUpdateTable<T> Set(Expression<Func<T, object>> column, object value)
        {
            Set(column.Body.GetPropertyName(), value);
            return this;
        }

        public void Set(string column, object value)
        {
            _builder.Append(string.Format(" {0}=@{1},", _dbProvider.EscapeName(_tableInfo.ResolveColumnName(column)), _builder.Parameters.NextIndex));
            _builder.Parameters.Add(value);
        }

        public void Where(Expression<Func<T, bool>> criteria)
        {
            _builder.Where(criteria);
        }

        public string GetSql()
        {
            return _builder.ToString();
        }

        public object[] GetParameters()
        {
            return _builder.Parameters.ToArray();
        }
    }
}