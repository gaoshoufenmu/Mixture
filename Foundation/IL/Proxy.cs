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
        #region Create Instance
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
        #endregion

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

        #region Clone
        private static Dictionary<Type, Delegate> _shallowDict = new Dictionary<Type, Delegate>();
        private static Dictionary<Type, Delegate> _deepDict = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Poor performance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T CloneByReflection<T>(T t) where T : new()
        {
            var flds = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var nt = new T();
            foreach (var fld in flds)
                fld.SetValue(nt, fld.GetValue(t));

            return nt;
        }

        public static T ShallowClone<T>(T  t)// where T : new()
        {
            var type = typeof(T);
            Delegate del = null;
            if(!_shallowDict.TryGetValue(type, out del))
            {
                lock(_shallowDict)
                {
                    if(!_shallowDict.TryGetValue(type, out del))
                    {
                        var dm = new DynamicMethod("ShallowClone" + type.Name, type, new Type[] { type }, Assembly.GetExecutingAssembly().ManifestModule, true);
                        var ctor = type.GetConstructor(new Type[] { });
                        var il = dm.GetILGenerator();
                        var lv = il.DeclareLocal(type);
                        il.Emit(OpCodes.Newobj, ctor);
                        il.Emit(OpCodes.Stloc_0);

                        foreach(var fld in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, fld);
                            il.Emit(OpCodes.Stfld, fld);
                        }
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ret);
                        del = dm.CreateDelegate(typeof(Func<T, T>));
                        _shallowDict[type] = del;
                    }
                }
            }
            return ((Func<T, T>)del)(t);
        }

        public static T DeepClone<T>(T t)
        {
            Delegate @delegate = null;
            Type type = typeof(T);
            if (!_deepDict.TryGetValue(type, out @delegate))
            {
                lock (_deepDict)
                {
                    if (!_deepDict.TryGetValue(type, out @delegate))
                    {
                        var dm = new DynamicMethod("DeepClone" + type.Name, typeof(T), new Type[] { type }, Assembly.GetExecutingAssembly().ManifestModule, true);
                        var ctorInfo = type.GetConstructor(new Type[] { });
                        var il = dm.GetILGenerator();
                        var lb = il.DeclareLocal(type);
                        il.Emit(OpCodes.Newobj, ctorInfo);  // new an instance of Type T
                        il.Emit(OpCodes.Stloc_0);           // pop out the new instance and save it at INDEX 0 of the local variable list

                        foreach (var fi in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                        {
                            if (fi.FieldType.IsValueType || fi.FieldType == typeof(string))
                            {
                                CopyValueType(il, fi);
                            }
                            else if (fi.FieldType.IsClass)
                            {
                                CopyReferenceType(il, fi);
                            }
                        }
                        il.Emit(OpCodes.Ldloc_0);   // load the new instance of Type T
                        il.Emit(OpCodes.Ret);       // return

                        @delegate = dm.CreateDelegate(typeof(Func<T, T>));
                        _deepDict[type] = @delegate;
                    }
                }
            }
            return ((Func<T, T>)@delegate)(t);
        }

        private static void CopyValueType(ILGenerator il, FieldInfo fi)
        {
            il.Emit(OpCodes.Ldloc_0);   // the new instance
            il.Emit(OpCodes.Ldarg_0);   // t
            il.Emit(OpCodes.Ldfld, fi); // get fi value of t
            il.Emit(OpCodes.Stfld, fi); // set fi value of t to the new instance
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="il"></param>
        /// <param name="fi">fi type is a reference type</param>
        private static void CopyReferenceType(ILGenerator il, FieldInfo fi)
        {
            var _lbfTemp = il.DeclareLocal(fi.FieldType);   // declare a variable of Type 'fi.FieldType'
            if (fi.FieldType.GetInterface("IEnumerable") != null)   // 'fi.FieldType' inherit the interface 'IEnumerable'
            {
                if (fi.FieldType.IsGenericType)         // if 'fi.FieldType' is a generic type
                {
                    // type of generic argument
                    var argType = fi.FieldType.GetGenericArguments()[0];
                    // get the concrete type
                    var genericType = Type.GetType("System.Collections.Generic.IEnumerable`1[" + argType.FullName + "]");

                    var ci = fi.FieldType.GetConstructor(new Type[] { genericType });   // get the .Ctor of Type 'List<argType>', denote that the items of the list are shallowly copied
                    if (ci != null)
                    {
                        il.Emit(OpCodes.Ldarg_0);           // load t
                        il.Emit(OpCodes.Ldfld, fi);         // get the fi value of t

                        // new a List<argType> instance, and push it to the stack. 
                        //Because the .Ctor's param is type 'List<argType>', and the param value is the fi value above which was push to the stack
                        il.Emit(OpCodes.Newobj, ci);

                        il.Emit(OpCodes.Stloc, _lbfTemp);   // assign the new instance above to _lbfTemp, and pop it out from the stack

                        il.Emit(OpCodes.Ldloc_0);           // load the new instance of T
                        il.Emit(OpCodes.Ldloc, _lbfTemp);   // load _lbfTemp
                        il.Emit(OpCodes.Stfld, fi);         // set _lbfTemp to the fi value of T
                    }
                }
            }
            else
            {
                var ctorInfo = fi.FieldType.GetConstructor(new Type[] { });     // get the .Ctor of Type 'fi.FieldType'
                il.Emit(OpCodes.Newobj, ctorInfo);  // new a fi value
                il.Emit(OpCodes.Stloc, _lbfTemp);   // assign the new fi value to _lbfTemp, and pop it out from the stack

                il.Emit(OpCodes.Ldloc_0);           // load the new instance of Type T
                il.Emit(OpCodes.Ldloc, _lbfTemp);   // load _lbfTemp
                il.Emit(OpCodes.Stfld, fi);         // set _lbfTemp to fi value of the new instance

                foreach (var f in fi.FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (f.FieldType.IsValueType || f.FieldType == typeof(string))
                    {
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldarg_0);       // load param t
                        il.Emit(OpCodes.Ldfld, fi);     // get fi value of t
                        il.Emit(OpCodes.Ldfld, f);      // get f value of fi value of t
                        il.Emit(OpCodes.Stfld, f);      // set f value of fi value of t to f value of _lbfTmep
                    }
                    else if (f.FieldType.IsClass)
                        CopyReferenceType(il, f);
                }
            }
        }
        #endregion


    }
}
