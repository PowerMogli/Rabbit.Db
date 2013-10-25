﻿using System;
using System.Data;
using RabbitDB.Attributes;

namespace RabbitDB.Mapping
{
    internal interface IPropertyInfo
    {
        ColumnAttribute ColumnAttribute { get; }

        DbType? DbType { get; set; }

        int Size { get; }

        object GetValue(object obj);

        void SetValue(object obj, object value);

        bool IsNullable { get; set; }

        bool CanWrite { get; }

        Type PropertyType { get; }

        string Name { get; }
    }
}