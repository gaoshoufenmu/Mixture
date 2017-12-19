using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;

namespace Foundation.IL
{
    public class Proxy
    {
        private static IDictionary<string, object> Store = new ConcurrentDictionary<string, object>();
        public static T CreateInstance<T>()
        {
            // return new T();      // if T : new ()
            Func<T> func;
            object obj;
            Type t = typeof(T);
            if(!Store.TryGetValue(t.FullName, out obj))
            {
                var dynMethod = new DynamicMethod("DM$OBJ_FACTORY_" + t.Name, t, null, t);
                var ilGen = dynMethod.GetILGenerator();
                ilGen.Emit(OpCodes.Newobj, t.GetConstructor(Type.EmptyTypes));
                ilGen.Emit(OpCodes.Ret);
                func = (Func<T>)dynMethod.CreateDelegate(typeof(Func<T>));
                Store[t.FullName] = func;
                return func();
            }
            func = (Func<T>)obj;
            return func();
        }

        public static T CreateInstance_lambda<T>()
        {
            Type t = typeof(T);
            object obj;
            Func<T> func;
            if (!Store.TryGetValue(t.FullName, out obj))
            {
                func = Expression.Lambda<Func<T>>(Expression.New(t.GetConstructor(Type.EmptyTypes))).Compile();
                Store[t.FullName] = func;
                return func();
            }
            func = (Func<T>)obj;
            return func();
        }

        public static T CreateInstance_lambda_args<T>(int arg)
        {
            Type t = typeof(T);
            object obj;
            Func<int, T> func;
            if (!Store.TryGetValue(t.FullName, out obj))
            {
                var arg_exp = Expression.Parameter(typeof(int), "arg");
                func = Expression.Lambda<Func<int, T>>(
                    Expression.New(t.GetConstructor(new[] { typeof(int) }), new[] { arg_exp }), arg_exp).Compile();
                Store[t.FullName] = func;
                return func(arg);
            }
            func = (Func<int, T>)obj;
            return func(arg);
        }

        /// <summary>
        /// 创建一个匿名类实例
        /// </summary>
        /// <param name="names">类成员名称</param>
        /// <param name="types">类成员类型</param>
        /// <param name="values">类成员值</param>
        /// <returns></returns>
        public static object CreateAnonymous(string[] names, Type[] types, object[] values)
        {
            var sb = new StringBuilder(names.Length * 5);
            foreach (var p in names)
                sb.Append(p);

            var tn = sb.ToString();
            object obj;
            if(!Store.TryGetValue(tn, out obj))
            {
                object obj_mb;
                if(!Store.TryGetValue("dyn_module_builder", out obj_mb))
                {
                    var assemble = new AssemblyName("dynamicassembly");
                    var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(assemble, AssemblyBuilderAccess.Run);
                    obj_mb = ab.DefineDynamicModule(assemble.Name, assemble.Name + ".dll");
                    Store["dyn_module_builder"] = obj_mb;
                }
                var mb = (ModuleBuilder)obj_mb;
                var tb = mb.DefineType(tn, TypeAttributes.Public);

                var ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, types);     // 定义默认构造函数
                var ilGen = ctor.GetILGenerator();

                // For a constructor, argument zero is a reference to the new
                // instance. Push it on the stack before calling the base
                // class constructor
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));       // 调用默认构造函数
                
                for(int i = 0; i < types.Length; i++)
                {
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldarg, i+1);
                    var fb = tb.DefineField(names[i], types[i], FieldAttributes.Public);
                    ilGen.Emit(OpCodes.Stfld, fb);
                }
                ilGen.Emit(OpCodes.Ret);

                var ctor_ = tb.CreateType().GetConstructor(types);
                Store[tn] = ctor_;
                return ctor_.Invoke(values);
            }
            var ctor__ = (ConstructorInfo)obj;
            return ctor__.Invoke(values);
        }

        #region DataRow 2 Entity
        private static readonly MethodInfo getterMethod = typeof(DataRow).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo isDBNullMethod = typeof(DataRow).GetMethod("IsNull", new Type[] { typeof(int) });
        public static T CreateEntity<T>(DataRow dr)
        {
            object obj;
            Type t = typeof(T);
            Func<DataRow, T> func;
            if(!Store.TryGetValue("DataRow2" + t.FullName, out obj))
            {
                var dynMethod = new DynamicMethod("DM$DataRow2" + t.FullName, t, new Type[] { typeof(DataRow) }, t, true);
                var ilGen = dynMethod.GetILGenerator();
                var result = ilGen.DeclareLocal(t);
                ilGen.Emit(OpCodes.Newobj, t.GetConstructor(Type.EmptyTypes));
                ilGen.Emit(OpCodes.Stloc, result);
                for(int i = 0; i < dr.ItemArray.Length; i++)
                {
                    var propInfo = t.GetProperty(dr.Table.Columns[i].ColumnName);
                    var label = ilGen.DefineLabel();
                    if(propInfo != null && propInfo.GetSetMethod() != null)
                    {
                        ilGen.Emit(OpCodes.Ldarg_0);
                        ilGen.Emit(OpCodes.Ldc_I4, i);
                        ilGen.Emit(OpCodes.Callvirt, isDBNullMethod);
                        ilGen.Emit(OpCodes.Brtrue, label);
                        ilGen.Emit(OpCodes.Ldloc, result);
                        ilGen.Emit(OpCodes.Ldarg_0);
                        ilGen.Emit(OpCodes.Ldc_I4, i);
                        ilGen.Emit(OpCodes.Callvirt, getterMethod);
                        var innerType = Nullable.GetUnderlyingType(propInfo.PropertyType);          // work out problems such as int? type.
                        if (innerType != null)
                            ilGen.Emit(OpCodes.Unbox_Any, innerType);
                        else
                            ilGen.Emit(OpCodes.Unbox_Any, propInfo.PropertyType);
                        ilGen.Emit(OpCodes.Callvirt, propInfo.GetSetMethod());
                        ilGen.MarkLabel(label);
                    }
                }
                ilGen.Emit(OpCodes.Ldloc, result);
                ilGen.Emit(OpCodes.Ret);
                func = (Func<DataRow, T>)dynMethod.CreateDelegate(typeof(Func<DataRow, T>));
                Store["DataRow2" + t.FullName] = func;
                return func(dr);
            }
            func = (Func<DataRow, T>)obj;
            return func(dr);
        }
        #endregion
    }
}
