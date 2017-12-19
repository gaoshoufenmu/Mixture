using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

namespace DesignPattern
{
    public interface ITask
    {
        string Name { get; }
        bool Start();
    }

    [InheritedExport]
    public interface IDBAccess<T>
    {
        List<T> SelectMany(Func<T, bool> where, int size);
        T SelectOne(Func<T, bool> first);

        void DeleteMany(Func<T, bool> where);
        void DeleteOne(Func<T, bool> first);
        void UpdateOne(Func<T, bool> first, Action<T> update);
        void InsertOne(T t);
    }
}
