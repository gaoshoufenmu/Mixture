using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.SqlClient;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using MefContrib.Hosting.Generics;
using Mef.Extended.Tools.Generics;

namespace DesignPattern
{
    [Export(typeof(ITask))]
    [ExportMetadata("key", "value")]
    public class Impl : ITask
    {
        [Import]
        protected IDBAccess<DBEntity> DBAccess { get; set; }
        [ImportMany]
        protected IEnumerable<IDBAccess<DBEntity>> dbAcceses;
        private IDBAccess<DBEntity> _dbaccese;
        protected IDBAccess<DBEntity> DBAccese
        {
            get
            {
                if (_dbaccese == null)
                    _dbaccese = dbAcceses.First();
                return _dbaccese;
            }
        }

        public string Name => "name";

        public Impl() =>
            Composition.GenericComposite().ComposeParts(this);


        public bool Start() =>
            DBAccess.SelectOne(e => e.id > 100) != null;
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DBAccess<T> : IDBAccess<T> where T : class
    {
        private DataContext _context;
        private Table<T> _table;

        [ImportingConstructor]
        public DBAccess()
        {
            _context = new DataContext($"connection string related with {typeof(T).Name}");
            _table = _context.GetTable<T>();
        }
        public List<T> SelectMany(Func<T, bool> where, int size) => _table.Where(where).Take(size).ToList();
        public T SelectOne(Func<T, bool> first) => _table.FirstOrDefault(first);

        public void DeleteMany(Func<T, bool> where) => DoAndSubmitChanges(() => _table.DeleteAllOnSubmit(_table.Where(where)));

        public void DeleteOne(Func<T, bool> first) => DoAndSubmitChanges(() => _table.DeleteOnSubmit(_table.First(first)));

        public void UpdateOne(Func<T, bool> first, Action<T> update) => DoAndSubmitChanges(() => update(_table.First(first)));

        public void InsertOne(T t) => DoAndSubmitChanges(() => _table.InsertOnSubmit(t));

        private void DoAndSubmitChanges(Action action)
        {
            action();
            _context.SubmitChanges();
        }
    }

    public class DBEntity
    {
        public int id { get; set; }
    }

    public static class Composition
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>type `container.ComposeParts(this)` at invoked place</remarks>
        /// <returns></returns>
        public static CompositionContainer Composite()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "DesignPattern*.dll"/*添加实现接口的dll文件名*/));
            return new CompositionContainer(catalog);
        }

        public static CompositionContainer GenericComposite()
        {
            //var ass = Directory.GetFileSystemEntries(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories);
            var registry = new GenericContractRegistry(new TypeResolver(x => Path.GetFileName(x).StartsWith("DesignPattern*.dll"/*添加实现接口的dll文件名*/)));
            var catalog = new GenericCatalog(registry);
            return new CompositionContainer(catalog);
        }
    }
}
