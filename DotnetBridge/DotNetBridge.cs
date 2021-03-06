#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          � West Wind Technologies, 2009
 *          http://www.west-wind.com/
 * 
 * Created: 4/10/2009
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Runtime.InteropServices;
using System.Reflection;
using Westwind.Utilities;
using System.IO;
using System.Data;
using System.Text;
using System.Windows.Forms;


namespace Westwind.WebConnection
{
    /// <summary>
    /// The wwDotNetBridge class provides a host of COM support functions for Visual FoxPro.
    /// It allows you to host the .NET runtime without relying on COM interop to load types,
    /// rather it acts as a proxy for instantiation and other tasks. 
    /// 
    /// This library can be used itself to load .NET and types, or you can use it as a helper
    /// with COM interop in which case you have to instantiate it as a COM object and call
    /// its methods directly rather than using the FoxPro helper class.
    /// </summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("Westwind.wwDotNetBridge")]
    public class wwDotNetBridge
    {
        /// <summary>
        /// Returns error information if the call fails
        /// </summary>
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }
        private string _ErrorMessage = "";

        public bool Error
        {
            get { return _Error; }
            set { _Error = value; }
        }
        private bool _Error = false;


        public Exception LastException { get; set; }


        #region LoadAssembly Routines
        /// <summary>
        /// Loads an assembly into the AppDomain by its fully qualified assembly name
        /// </summary>
        /// <param name="AssemblyName"></param>
        /// <returns></returns>
        public bool LoadAssembly(string AssemblyName)
        {
            // *** use some predefined names to keep things simpler
            AssemblyName = FixupAssemblyName(AssemblyName);
            Assembly ass = null;
            try
            {
                ass = Assembly.Load(AssemblyName);
            }
            catch (Exception ex)
            {
                LastException = ex;
                this.ErrorMessage = ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads an assembly into the AppDomain by a fully qualified assembly path
        /// </summary>
        /// <param name="AssemblyFileName"></param>
        /// <returns></returns>
        public bool LoadAssemblyFrom(string AssemblyFileName)
        {
            try
            {
                Assembly.LoadFrom(AssemblyFileName);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    LastException = ex.InnerException;
                    this.SetError(ex.InnerException.Message);
                }
                else
                {
                    LastException = ex;
                    this.SetError(ex.Message);
                }
                return false;
            }

            return true;
        }
        #endregion

        #region CreateInstance by type name only
        /// <summary>
        /// Creates a type reference from a given type name if the
        /// assembly is already loaded.
        /// </summary>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public object CreateInstance(string TypeName)
        {
            object obj = this.CreateInstance_Internal(TypeName);
            return obj;
        }

        /// <summary>
        /// Creates a type reference from a given type name if the
        /// assembly is already loaded.
        /// </summary>        
        public object CreateInstance_OneParm(string TypeName, object Parm1)
        {
            Parm1 = this.FixupParameter(Parm1);

            return this.CreateInstance_Internal(TypeName, Parm1);
        }

        /// <summary>
        /// Creates a type reference from a given type name if the
        /// assembly is already loaded.
        /// </summary>
        public object CreateInstance_TwoParms(string TypeName, object Parm1, object Parm2)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);

            return this.CreateInstance_Internal(TypeName, Parm1, Parm2);
        }
        /// <summary>
        /// Creates a type reference from a given type name if the
        /// assembly is already loaded.
        /// </summary>
        public object CreateInstance_ThreeParms(string TypeName, object Parm1, object Parm2, object Parm3)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);

            return this.CreateInstance_Internal(TypeName, Parm1, Parm2, Parm3);
        }

        public object CreateInstance_FourParms(string TypeName, object Parm1, object Parm2, object Parm3, object Parm4)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);

            return this.CreateInstance_Internal(TypeName, Parm1, Parm2, Parm3, Parm4);
        }
        public object CreateInstance_FiveParms(string TypeName, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);
            Parm5 = this.FixupParameter(Parm5);

            return this.CreateInstance_Internal(TypeName, Parm1, Parm2, Parm3, Parm4, Parm5);
        }

        /// <summary>
        /// Creates an instance of a class  based on its type name. Assumes that the type's
        /// assembly is already loaded.
        /// 
        /// Note this will be a little slower than the versions that work with assembly
        /// name specified because this code has to search for the type first rather
        /// than directly activating it.
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected internal object CreateInstance_Internal(string TypeName, params object[] args)
        {
            this.SetError();

            object instance = null;
            Type type = null;
            try
            {
                type = this.GetTypeFromName(TypeName);

                if (type == null)
                {
                    this.SetError("Type not loaded. Please load call LoadAssembly first.");
                    return null;
                }

                instance = type.Assembly.CreateInstance(TypeName, false, BindingFlags.Default, null, args, null, null);
            }
            catch (TargetInvocationException ex)
            {
                SetError(ex, true);
                return null;
            }
            catch (Exception ex)
            {
                this.SetError(ex, true);
                return null;
            }

            return this.FixupReturnValue(instance);
        }


        /// <summary>
        /// Routine that loads an assembly by its 'application assembly name' - unsigned
        /// assemblies must be visible via the .NET path (current path or BIN dir) and
        /// GAC assemblies must be referenced by their full assembly name.
        /// </summary>
        /// <param name="AssemblyName"></param>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        protected object CreateInstance_Internal(string AssemblyName, string TypeName, params object[] args)
        {
            this.SetError();

            AssemblyName = FixupAssemblyName(AssemblyName);

            object instance = null;
            try
            {
                if (args == null)
                    instance = AppDomain.CurrentDomain.CreateInstanceAndUnwrap(AssemblyName, TypeName);
                else
                    instance = AppDomain.CurrentDomain.CreateInstanceAndUnwrap(AssemblyName, TypeName, false, BindingFlags.Default, null, args, null, null, null);
            }
            catch (TargetInvocationException ex)
            {
                SetError(ex, true);
                return null;
            }
            catch (Exception ex)
            {
                this.SetError(ex, true);
                return null;
            }

            return this.FixupReturnValue(instance);
        }

        /// <summary>
        /// Creates an instance of a .NET type and stores it on an existing property of another type.
        /// 
        /// Use this method if you can't access a type through COM ([ComVisible(false)]
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="property"></param>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public bool CreateInstanceOnType(object instance, string property, string TypeName)
        {
            return this.CreateInstanceOnType_Internal(instance, property, TypeName);
        }
        public bool CreateInstanceOnType_OneParm(object instance, string property, string TypeName, object parm1)
        {
            return this.CreateInstanceOnType_Internal(instance, property, TypeName, parm1);
        }
        public bool CreateInstanceOnType_TwoParms(object instance, string property, string TypeName, object parm1, object parm2)
        {
            return this.CreateInstanceOnType_Internal(instance, property, TypeName, parm1, parm2);
        }
        public bool CreateInstanceOnType_ThreeParms(object instance, string property, string TypeName, object parm1, object parm2, object parm3)
        {
            return this.CreateInstanceOnType_Internal(instance, property, TypeName, parm1, parm2, parm3);
        }
        public bool CreateInstanceOnType_FourParms(object instance, string property, string TypeName, object parm1, object parm2, object parm3, object parm4)
        {
            return this.CreateInstanceOnType_Internal(instance, property, TypeName, parm1, parm2, parm3, parm4);
        }
        public bool CreateInstanceOnType_FiveParms(object instance, string property, string TypeName, object parm1, object parm2, object parm3, object parm4, object parm5)
        {
            return this.CreateInstanceOnType_Internal(instance, property, TypeName, parm1, parm2, parm3, parm4, parm5);
        }
        protected bool CreateInstanceOnType_Internal(object instance, string property, string TypeName, params object[] args)
        {
            this.SetError();

            object newInstance = null;
            Type type = null;
            try
            {
                type = this.GetTypeFromName(TypeName);

                if (type == null)
                {
                    this.SetError("Type not loaded. Please load call LoadAssembly first.");
                    return false;
                }

                newInstance = type.Assembly.CreateInstance(TypeName, false, BindingFlags.Default, null, args, null, null);
            }
            catch (Exception ex)
            {
                LastException = ex;
                this.SetError(ex.Message);
                return false;
            }

            try
            {
                ReflectionUtils.SetPropertyExCom(instance, property, newInstance);
            }
            catch (Exception ex)
            {
                LastException = ex;
                this.ErrorMessage = ex.Message;
                return false;
            }

            return true;
        }

        #endregion

        #region Load instance by assembly name and type
        /// <summary>
        /// Creates an instance from a file reference with a parameterless constructor
        /// </summary>
        /// <param name="AssemblyFileName"></param>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public object CreateAssemblyInstanceFromFile(string AssemblyFileName, string TypeName)
        {
            return this.CreateInstanceFromFile_Internal(AssemblyFileName, TypeName, null);
        }

        /// <summary>
        /// Creates an instance from a file reference with a 1 parameter constructor
        /// </summary>
        /// <param name="AssemblyFileName"></param>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public object CreateAssemblyInstanceFromFile_OneParm(string AssemblyFileName, string TypeName, object Parm1)
        {
            return this.CreateInstanceFromFile_Internal(AssemblyFileName, TypeName, new Object[1] { Parm1 });
        }

        /// <summary>
        /// Creates an instance from a file reference with a two parameter constructor
        /// </summary>
        /// <param name="AssemblyFileName"></param>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public object CreateAssemblyInstanceFromFile_TwoParms(string AssemblyFileName, string TypeName, object Parm1, object Parm2)
        {
            return this.CreateInstanceFromFile_Internal(AssemblyFileName, TypeName, new Object[2] { Parm1, Parm2 });
        }


        /// <summary>
        /// Routine that loads a class from an assembly file name specified.
        /// </summary>
        /// <param name="AssemblyFileName"></param>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        protected object CreateInstanceFromFile_Internal(string AssemblyFileName, string TypeName, params object[] args)
        {
            this.ErrorMessage = string.Empty;
            LastException = null;

            object server = null;

            try
            {
                if (args == null)
                    server = AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(AssemblyFileName, TypeName);
                else
                    server = AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(AssemblyFileName, TypeName, false, BindingFlags.Default, null, args, null, null, null);
            }
            catch (Exception ex)
            {
                LastException = ex;
                if (ex.InnerException != null)
                {
                    LastException = ex.InnerException;
                    SetError(ex.InnerException.Message);
                }
                else
                    SetError(ex.Message);

                return null;
            }

            return server;
        }


        /// <summary>
        /// Creates a new instance from a file file based assembly refence. Requires full
        /// filename including extension and path.
        /// </summary>
        /// <param name="AssemblyFileName"></param>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public object CreateAssemblyInstance(string AssemblyFileName, string TypeName)
        {
            return this.CreateInstance_Internal(AssemblyFileName, TypeName, null);
        }
        /// <summary>
        /// Creates a new instance from a file file based assembly refence. Requires full
        /// filename including extension and path.
        /// </summary>
        /// <param name="AssemblyFileName"></param>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public object CreateAssemblyInstance_OneParm(string AssemblyFileName, string TypeName, object Parm1)
        {
            return this.CreateInstance_Internal(AssemblyFileName, TypeName, Parm1);
        }
        /// <summary>
        /// Creates a new instance from a file file based assembly refence. Requires full
        /// filename including extension and path.
        /// </summary>
        /// <param name="AssemblyFileName"></param>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public object CreateAssemblyInstance_TwoParms(string AssemblyFileName, string TypeName, object Parm1, object Parm2)
        {
            return this.CreateInstance_Internal(AssemblyFileName, TypeName, Parm1, Parm2);
        }


        #endregion

        public object InvokeStaticMethod(string TypeName, string Method)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method);
        }


        public object InvokeStaticMethod_OneParm(string TypeName, string Method, object Parm1)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1);
        }


        public object InvokeStaticMethod_TwoParms(string TypeName, string Method, object Parm1, object Parm2)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1, Parm2);
        }

        public object InvokeStaticMethod_ThreeParms(string TypeName, string Method, object Parm1, object Parm2, object Parm3)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1, Parm2, Parm3);
        }
        public object InvokeStaticMethod_FourParms(string TypeName, string Method, object Parm1, object Parm2, object Parm3, object Parm4)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1, Parm2, Parm3, Parm4);
        }
        public object InvokeStaticMethod_FiveParms(string TypeName, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1, Parm2, Parm3, Parm4, Parm5);
        }
        public object InvokeStaticMethod_SixParms(string TypeName, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6);
        }
        public object InvokeStaticMethod_SevenParms(string TypeName, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7);
        }
        public object InvokeStaticMethod_EightParms(string TypeName, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7, object Parm8)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7, Parm8);
        }
        public object InvokeStaticMethod_NineParms(string TypeName, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7, object Parm8, object Parm9)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7, Parm8, Parm9);
        }
        public object InvokeStaticMethod_TenParms(string TypeName, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7, object Parm8, object Parm9, object Parm10)
        {
            return this.InvokeStaticMethod_Internal(TypeName, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7, Parm8, Parm9, Parm10);
        }

        /// <summary>
        /// Invokes a static method
        /// </summary>
        /// <param name="TypeName"></param>
        /// <param name="Method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected object InvokeStaticMethod_Internal(string TypeName, string Method, params object[] args)
        {
            this.SetError();

            Type type = this.GetTypeFromName(TypeName);
            if (type == null)
            {
                this.SetError("Type is not loaded. Please make sure you call LoadAssembly first.");
                return null;
            }

            // fix up all parameters
            Array ar = args as Array;
            for (int i = 0; i < args.Length; i++)
            {
                ar.SetValue(this.FixupParameter(args[i]), i);
            }

            this.ErrorMessage = "";
            object Result = null;
            try
            {
                Result = type.InvokeMember(Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, type, args);
            }
            catch (Exception ex)
            {
                this.SetError(ex, true);
                return null;
            }

            return this.FixupReturnValue(Result);
        }

        /// <summary>
        /// Retrieves a value from  a static property by specifying a type full name and property
        /// </summary>
        /// <param name="TypeName">Full type name (namespace.class)</param>
        /// <param name="Property">Property to get value from</param>
        /// <returns></returns>
        public object GetStaticProperty(string TypeName, string Property)
        {
            this.SetError();

            Type type = this.GetTypeFromName(TypeName);
            if (type == null)
            {
                this.SetError("Type is not loaded. Please make sure you call LoadAssembly first.");
                return null;
            }

            object Result = null;
            try
            {
                Result = type.InvokeMember(Property, BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty, null, type, null);
            }
            catch (Exception ex)
            {
                this.SetError(ex, true);
                return null;
            }
            return Result;
        }

        public bool SetStaticProperty(string TypeName, string Property, object Value)
        {
            this.SetError();

            Type type = this.GetTypeFromName(TypeName);
            if (type == null)
            {
                this.SetError("Unable to find type signature. Please make sure you call LoadAssembly first.");
                return false;
            }

            this.ErrorMessage = "";
            object Result = null;
            try
            {
                Result = type.InvokeMember(Property, BindingFlags.Static | BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty, null, type, new object[1] { Value });
            }
            catch (Exception ex)
            {
                this.SetError(ex, true);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns the name of an enum field given an enum value
        /// passed. Pass in the name of the enum type
        /// </summary>
        /// <param name="EnumTypeName"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public string GetEnumString(string EnumTypeName, object Value)
        {
            this.SetError();
            try
            {
                Type type = this.GetTypeFromName(EnumTypeName);
                return Enum.GetName(type, Value);
            }
            catch
            {
                this.SetError("Invalid enum value");
            }

            return null;
        }

        public Type GetType(object value)
        {
            if (value == null)
                return null;

            return value.GetType();
        }

        /// <summary>
        /// Helper routine that looks up a type name and tries to retrieve the
        /// full type reference in the actively executing assemblies.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public Type GetTypeFromName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            Type type = null;
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = ass.GetType(typeName, false);
                if (type != null)
                    break;
            }
            return type;
        }

        /// <summary>
        /// Invokes a method with no parameters
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="Method"></param>
        /// <returns></returns>
        public object InvokeMethod(object Instance, string Method)
        {
            return this.InvokeMethod_Internal(Instance, Method);
        }


        public object InvokeMethodWithParameterArray(object instance, string method, object[] parms)
        {
            return this.InvokeMethod_InternalWithObjectArray(instance, method, parms);
        }


        /// <summary>
        /// Invokes a method with one parameter
        /// </summary> 
        /// <param name="Instance"></param>
        /// <param name="Method"></param>
        /// <param name="Parm1"></param>
        /// <returns></returns>
        public object InvokeMethod_OneParm(object Instance, string Method, object Parm1)
        {
            Parm1 = FixupParameter(Parm1);

            return this.InvokeMethod_Internal(Instance, Method, Parm1);
        }

        /// <summary>
        /// Invokes a method with two parameters
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="Method"></param>
        /// <param name="Parm1"></param>
        /// <param name="Parm2"></param>
        /// <returns></returns>
        public object InvokeMethod_TwoParms(object Instance, string Method, object Parm1, object Parm2)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2);
        }

        public object InvokeMethod_ThreeParms(object Instance, string Method, object Parm1, object Parm2, object Parm3)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3);
        }
        public object InvokeMethod_FourParms(object Instance, string Method, object Parm1, object Parm2, object Parm3, object Parm4)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3, Parm4);
        }
        public object InvokeMethod_FiveParms(object Instance, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);
            Parm5 = this.FixupParameter(Parm5);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3, Parm4, Parm5);
        }
        public object InvokeMethod_SixParms(object Instance, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);
            Parm5 = this.FixupParameter(Parm5);
            Parm6 = this.FixupParameter(Parm6);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6);
        }
        public object InvokeMethod_SevenParms(object Instance, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);
            Parm5 = this.FixupParameter(Parm5);
            Parm6 = this.FixupParameter(Parm6);
            Parm7 = this.FixupParameter(Parm7);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7);
        }
        public object InvokeMethod_EightParms(object Instance, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7, object Parm8)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);
            Parm5 = this.FixupParameter(Parm5);
            Parm6 = this.FixupParameter(Parm6);
            Parm7 = this.FixupParameter(Parm7);
            Parm8 = this.FixupParameter(Parm8);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7, Parm8);
        }
        public object InvokeMethod_NineParms(object Instance, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7, object Parm8, object Parm9)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);
            Parm5 = this.FixupParameter(Parm5);
            Parm6 = this.FixupParameter(Parm6);
            Parm7 = this.FixupParameter(Parm7);
            Parm8 = this.FixupParameter(Parm8);
            Parm9 = this.FixupParameter(Parm9);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7, Parm8, Parm9);
        }
        public object InvokeMethod_TenParms(object Instance, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7, object Parm8, object Parm9, object Parm10)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);
            Parm5 = this.FixupParameter(Parm5);
            Parm6 = this.FixupParameter(Parm6);
            Parm7 = this.FixupParameter(Parm7);
            Parm8 = this.FixupParameter(Parm8);
            Parm9 = this.FixupParameter(Parm9);
            Parm10 = this.FixupParameter(Parm10);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7, Parm8, Parm9, Parm10);
        }
        public object InvokeMethod_ElevenParms(object Instance, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7, object Parm8, object Parm9, object Parm10, object Parm11)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);
            Parm5 = this.FixupParameter(Parm5);
            Parm6 = this.FixupParameter(Parm6);
            Parm7 = this.FixupParameter(Parm7);
            Parm8 = this.FixupParameter(Parm8);
            Parm9 = this.FixupParameter(Parm9);
            Parm10 = this.FixupParameter(Parm10);
            Parm11 = this.FixupParameter(Parm11);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7, Parm8, Parm9, Parm10, Parm11);
        }
        public object InvokeMethod_TwelveParms(object Instance, string Method, object Parm1, object Parm2, object Parm3, object Parm4, object Parm5, object Parm6, object Parm7, object Parm8, object Parm9, object Parm10, object Parm11, object Parm12)
        {
            Parm1 = this.FixupParameter(Parm1);
            Parm2 = this.FixupParameter(Parm2);
            Parm3 = this.FixupParameter(Parm3);
            Parm4 = this.FixupParameter(Parm4);
            Parm5 = this.FixupParameter(Parm5);
            Parm6 = this.FixupParameter(Parm6);
            Parm7 = this.FixupParameter(Parm7);
            Parm8 = this.FixupParameter(Parm8);
            Parm9 = this.FixupParameter(Parm9);
            Parm10 = this.FixupParameter(Parm10);
            Parm11 = this.FixupParameter(Parm11);
            Parm12 = this.FixupParameter(Parm12);

            return this.InvokeMethod_Internal(Instance, Method, Parm1, Parm2, Parm3, Parm4, Parm5, Parm6, Parm7, Parm8, Parm9, Parm10, Parm11, Parm12);
        }

        public object InvokeMethod_ParameterArray(object Instance, string Method, object ParameterArray)
        {
            ComArray ParmArray = ParameterArray as ComArray;
            for (int i = 0; i < ParmArray.Count; i++)
            {
                Array ar = ParmArray.Instance as Array;
                ar.SetValue(this.FixupParameter(ar.GetValue(i)), i);
            }
            return this.InvokeMethod_InternalWithObjectArray(Instance, Method, ParmArray.Instance as object[]);
        }

        public object GetProperty(object Instance, string Property)
        {
            LastException = null;
            try
            {
                object val = ReflectionUtils.GetPropertyCom(Instance, Property);
                val = this.FixupReturnValue(val);
                return val;
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                throw ex.GetBaseException();
            }
        }

        public object GetPropertyEx(object Instance, string Property)
        {
            LastException = null;
            try
            {
                object val = ReflectionUtils.GetPropertyEx(Instance, Property);
                val = this.FixupReturnValue(val);
                return val;
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                throw ex.GetBaseException();
            }
        }

        /// <summary>
        /// Sets a property of a .NET object with a value
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="Property"></param>
        /// <param name="Value"></param>
        public void SetProperty(ref object Instance, string Property, object Value)
        {
            LastException = null;

            if (Value is System.DBNull)
                Value = null;

            Value = this.FixupParameter(Value);
            try
            {
                ReflectionUtils.SetPropertyCom(Instance, Property, Value);
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                throw ex.GetBaseException();
            }
        }

        /// <summary>
        /// Sets a property of a .NET object with a value using extended syntax.
        /// 
        /// This method supports '.' syntax so you can use "Property.ChildProperty"
        /// to walk the object hierarchy in the string property parameter. 
        /// 
        /// This method also supports accessing of Array/Collection indexers (Item[1])
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="Property"></param>
        /// <param name="Value"></param>
        public void SetPropertyEx(ref object Instance, string Property, object Value)
        {
            LastException = null;
            try
            {
                if (Value is System.DBNull)
                    Value = null;

                Value = this.FixupParameter(Value);
                ReflectionUtils.SetPropertyEx(Instance, Property, Value);
            }
            catch (Exception ex)
            {
                LastException = ex;
                this.SetError(ex, true);
                throw ex.GetBaseException();
            }
        }

        //public object InvokeMethod_OneParm(string Method, object Parm1)
        //{
        //    return InvokeMethod_Internal(Method, Parm1);
        //}
        //public object InvokeMethod_OneParm(string Method, object Parm1)
        //{
        //    return InvokeMethod_Internal(Method, Parm1);
        //}

        protected object InvokeMethod_Internal(object Instance, string Method, params object[] args)
        {
            Array ar = args as Array;
            for (int i = 0; i < args.Length; i++)
            {
                ar.SetValue(this.FixupParameter(args[i]), i);
            }

            this.SetError();
            object result = null;
            try
            {
                result = ReflectionUtils.CallMethodCom(Instance, Method, args);
            }
            catch (Exception ex)
            {
                SetError(ex.GetBaseException(), true);
                throw ex.GetBaseException();
            }
            return this.FixupReturnValue(result);
        }


        protected object InvokeMethod_InternalWithObjectArray(object Instance, string Method, object[] args)
        {
            this.SetError();
            object result;

            try
            {
                result = ReflectionUtils.CallMethodCom(Instance, Method, args);
            }
            catch (Exception ex)
            {
                SetError(ex, true);
                throw ex.GetBaseException();
            }

            return this.FixupReturnValue(result);
        }

        #region Array Functions

        /// <summary>
        /// Creates an instance of an array on a given base object instance by name.
        /// Array is created with 'empty' elements - ie. objects are null and value
        /// types are set to their default() values.
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="arrayProperty"></param>
        /// <returns></returns>
        private Array CreateArrayInstanceInternal(object baseType, string arrayProperty, int size)
        {
            MemberInfo[] miArray = baseType.GetType().GetMember(arrayProperty);
            if (miArray == null || miArray.Length == 0)
            {
                this.SetError("Array member doesn't exist on base type");
                return null;
            }

            Type type = null;
            MemberInfo mi = miArray[0];
            if (mi.MemberType == MemberTypes.Field)
                type = baseType.GetType().GetField(arrayProperty).FieldType.GetElementType();
            else
                type = baseType.GetType().GetProperty(arrayProperty).PropertyType.GetElementType();

            if (type == null)
            {
                this.SetError("Invalid type for array: " + type.Name);
                return null;
            }

            // *** Create instance and assign size
            Array ar = Array.CreateInstance(type, size);
            return ar;
        }

        /// <summary>
        /// Creates an array instance of a given type and size. Note the
        /// elements of this array are null/default and need to be set explicitly
        /// </summary>
        /// <param name="baseType">Object instance on which to create the array</param>
        /// <param name="arrayProperty">String property/field name of the array to create</param>
        /// <param name="size">Size of the array to createArray</param>
        /// <returns></returns>
        public bool CreateArrayOnInstance(object baseType, string arrayProperty, int size)
        {
            this.SetError();

            Array ar = CreateArrayInstanceInternal(baseType, arrayProperty, size);
            if (ar == null)
                return false;

            ReflectionUtils.SetPropertyExCom(baseType, arrayProperty, ar);

            return true;
        }

        /// <summary>
        /// Creates a new array instance on a type of exactly 1 array item which is
        /// assigned the item parameter passed in. 
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="arrayProperty"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CreateArrayOnInstanceWithObject(object baseType, string arrayProperty, object item)
        {
            this.SetError();

            // *** Create instance and assign
            Array ar = this.CreateArrayInstanceInternal(baseType, arrayProperty, 1);
            if (ar == null)
                return false;

            // *** assign the item passed in
            ar.SetValue(item, 0);

            ReflectionUtils.SetPropertyExCom(baseType, arrayProperty, ar);

            return true;
        }

        /// <summary>
        /// Creates an instance of an array
        /// </summary>
        /// <param name="arrayTypeString"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public ComArray CreateArray(string arrayTypeString)
        {
            this.SetError();

            Type type = null;
            type = ReflectionUtils.GetTypeFromName(arrayTypeString);

            if (type == null)
            {
                this.SetError("Invalid type for array: " + arrayTypeString);
                return null;
            }

            ComArray comArray = new ComArray();

            // *** Create instance and assign
            Array ar = Array.CreateInstance(type, 0);

            // *** assign the item passed in
            //ar.SetValue(item, 0);
            comArray.Instance = ar;

            return comArray;
        }

        /// <summary>
        /// Creates an array from a specific instance of a COM object
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public ComArray CreateArrayFromInstance(object instance)
        {
            return new ComArray(instance);
        }


        /// <summary>
        /// Used to add an item to an array by indirection to work around VFP's
        /// inability to easily add array elements.
        /// </summary>
        /// <param name="baseObject">The object that has the Array property</param>
        /// <param name="arrayObject">The array property name as a string</param>
        /// <param name="item">The item to set it to. Should not be null.</param>
        /// <returns></returns>
        public bool AddArrayItem(object baseObject, string arrayObject, object item)
        {
            this.SetError();

            Array ar = baseObject.GetType().InvokeMember(arrayObject,
                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.GetField,
                                   null, baseObject, null) as Array;

            // *** Null assignments are not allowed because we may have to create the array
            // *** and a type is required for that. If necessary create an empty instance
            if (item == null && ar == null)
                return false;


            // *** This may be ambigous - could mean no property or array exists and is null
            if (ar == null)
            {
                ar = Array.CreateInstance(item.GetType(), 1);
                if (ar == null)
                    return false;

                ar.SetValue(item, 0);
            }
            else
            {
                Type itemType = ar.GetType().GetElementType();

                int Size = ar.GetLength(0);  // one dimensional

                // resize the array by creating a new one
                Array TempArray = Array.CreateInstance(itemType, Size + 1);
                ar.CopyTo(TempArray, 0);

                // reassign to original instance var
                ar = TempArray;

                // and assign the new added item
                ar.SetValue(item, Size);
            }

            // make sure the original array instance reference gets updated
            // both for new array and updated array!
            baseObject.GetType().InvokeMember(arrayObject,
                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.SetField,
                                  null, baseObject, new object[1] { ar });

            return true;
        }

        /// <summary>
        /// Returns an individual Array Item by its index
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="arrayName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public object GetArrayItem(object baseObject, string arrayName, int index)
        {
            this.SetError();

            Array ar = baseObject.GetType().InvokeMember(arrayName,
                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.GetField,
                                   null, baseObject, null) as Array;

            // *** Null assignments are not allowed because we may have to create the array
            // *** and a type is required for that. If necessary create an empty instance
            if (ar == null)
                return false;

            return ar.GetValue(index);
        }

        /// <summary>
        /// Sets an array element to a given value. Assumes the array is big
        /// enough and the array item exists.
        /// </summary>
        /// <param name="baseObject">base object reference</param>
        /// <param name="arrayName">Name of the array as a string</param>
        /// <param name="index">The index of the item to set</param>
        /// <param name="value">The value to set the array item to</param>
        /// <returns></returns>
        public bool SetArrayItem(object baseObject, string arrayName, int index, object value)
        {
            this.SetError();

            Array ar = baseObject.GetType().InvokeMember(arrayName,
                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.GetField,
                                   null, baseObject, null) as Array;

            // *** Null assignments are not allowed because we may have to create the array
            // *** and a type is required for that. If necessary create an empty instance
            if (ar == null)
                return false;

            ar.SetValue(value, index);

            return true;
        }


        /// <summary>
        /// Removes an item from a .NET array with indirection to work around VFP's
        /// inability to manipulate .NET array elements.
        /// </summary>
        /// <param name="baseObject">The arrays parent object</param>
        /// <param name="arrayObject">The array's name as a string</param>
        /// <param name="Index">The index to of the item to delete. NOTE: 1 based!</param>
        /// <returns></returns>
        public bool RemoveArrayItem(object baseObject, string arrayObject, int Index)
        {
            this.SetError();

            // *** Assume 1 based arrays are passed in
            Index--;

            // *** USe Reflection to get a reference to the array Property
            Array ar = baseObject.GetType().InvokeMember(arrayObject,
                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.GetField,
                       null, baseObject, null) as Array;

            // *** This may be ambigous - could mean no property or array exists and is null
            if (ar == null)
                return false;

            int arSize = ar.GetLength(0);

            if (arSize < 1)
                return false;

            Type arType = ar.GetValue(0).GetType();

            Array NewArray = Array.CreateInstance(arType, arSize - 1);

            // *** Manually copy the array
            int NewCount = 0;
            for (int i = 0; i < arSize; i++)
            {
                if (i != Index)
                {
                    NewArray.SetValue(ar.GetValue(i), NewCount);
                    NewCount++;
                }
            }

            baseObject.GetType().InvokeMember(arrayObject,
                                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.SetField,
                                   null, baseObject, new object[1] { NewArray });

            return true;
        }

        /// <summary>
        /// Returns an XML string from a .NET DataSet
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="includeSchema"></param>
        /// <returns></returns>
        public string DataSetToXmlString(DataSet ds, bool includeSchema)
        {
            this.SetError();

            MemoryStream ms = new MemoryStream(4096);
            ds.WriteXml(ms, includeSchema ? XmlWriteMode.WriteSchema : XmlWriteMode.IgnoreSchema);
            string xml = Encoding.UTF8.GetString(ms.ToArray());
            return xml;
        }

        /// <summary>
        /// Converts an Xml String created from a FoxPro Xml Adapter or CursorToXml
        /// into a DataSet if possible.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public DataSet XmlStringToDataSet(string xml)
        {
            this.SetError();

            if (string.IsNullOrEmpty(xml))
                return null;

            try
            {
                using (MemoryStream ms = new MemoryStream(4096))
                {
                    byte[] xmlBuffer = Encoding.UTF8.GetBytes(xml);
                    ms.Write(xmlBuffer, 0, xmlBuffer.Length);
                    ms.Position = 0;

                    DataSet ds = new DataSet();
                    ds.ReadXml(ms);
                    return ds;
                }
            }
            catch (Exception ex)
            {
                this.SetError(ex.Message);
                return null;
            }
        }

        

        #endregion

        #region TypeConversionRoutines

        private object FixupParameter(object val)
        {
            if (val == null)
                return null;

            Type type = val.GetType();

            // *** Fix up binary SafeArrays into byte[]
            if (type.Name == "Byte[*]")
                return ConvertObjectToByteArray(val);

            // if we're dealing with ComValue parameter/value
            // just use it's Value property
            if (type == typeof(ComValue))
                return ((ComValue)val).Value;
            else if (type == typeof(ComGuid))
                return ((ComGuid)val).Guid;
            else if (type == typeof(ComArray))
                return ((ComArray)val).Instance;

            return val;
        }

        /// <summary>
        /// Fixes up a return value based on its type
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private object FixupReturnValue(object val)
        {
            if (val == null)
                return null;

            // *** Need to figure out a solution for value types
            Type type = val.GetType();

            if (type == typeof(Guid))
            {
                ComGuid guid = new ComGuid();
                guid.Guid = (Guid)val;
                return guid;
            }
            else if (type == typeof(long) || type == typeof(Int64))
                return Convert.ToDecimal(val);
            else if (type == typeof(byte[]))
            {
                // this ensures byte[] is not treated like an array (below)                
            }
            // FoxPro can't deal with DBNull as it's a value type
            else if (type == typeof(DBNull))
            {
                val = null;
            }
            else if (type.IsArray)
            {
                ComArray comArray = new ComArray();
                comArray.Instance = val as Array;
                return comArray;
            }

            return val;
        }

        /// <summary>
        /// Converts an object to a byte array
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public byte[] ConvertObjectToByteArray(object val)
        {
            Array ct = (Array)val;
            Byte[] content = new byte[ct.Length];
            ct.CopyTo(content, 0);
            return content;
        }



        #endregion


        /// <summary>
        /// Helper routine that automatically assigns default names to certain
        /// 'common' system assemblies so that we don't have to provide a full path
        /// 
        /// NOTE: 
        /// All names are for .NET 2.0 Runtime at the moment
        /// </summary>
        /// <param name="AssemblyName"></param>
        protected string FixupAssemblyName(string AssemblyName)
        {
            string lowerAssemblyName = AssemblyName.ToLower();

            if (Environment.Version.Major == 2)
            {
                if (lowerAssemblyName == "system")
                    AssemblyName = "System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "mscorlib")
                    AssemblyName = "mscorlib, Version=2.1.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "system.windows.forms")
                    AssemblyName = "System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "system.xml")
                    AssemblyName = "System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "system.drawing")
                    AssemblyName = "System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
                else if (lowerAssemblyName == "system.data")
                    AssemblyName = "System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "system.web")
                    AssemblyName = "System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            }
            else if (Environment.Version.Major == 4)
            {
                if (lowerAssemblyName == "system")
                    AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "mscorlib")
                    AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "system.windows.forms")
                    AssemblyName = "System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "system.xml")
                    AssemblyName = "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "system.drawing")
                    AssemblyName = "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
                else if (lowerAssemblyName == "system.data")
                    AssemblyName = "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "system.web")
                    AssemblyName = "System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
                else if (lowerAssemblyName == "system.core")
                    AssemblyName = "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "microsoft.csharp")
                    AssemblyName = "Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
                else if (lowerAssemblyName == "microsoft.visualbasic")
                    AssemblyName = "Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
                else if (lowerAssemblyName == "system.servicemodel")
                    AssemblyName = "System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                else if (lowerAssemblyName == "system.runtime.serialization")
                    AssemblyName = "System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            }

            return AssemblyName;
        }

        protected void SetError()
        {
            this.SetError(string.Empty);
        }

        protected void SetError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                LastException = null;
                Error = false;
                ErrorMessage = "";
                return;
            }
            Error = true;
            ErrorMessage = message;
        }

        protected void SetError(Exception ex, bool checkInner)
        {
            if (ex == null)
            {
                this.Error = false;
                this.ErrorMessage = string.Empty;
                this.LastException = null;
            }

            Exception e = ex;
            if (checkInner)
                e = ex.GetBaseException();

            this.Error = true;
            this.ErrorMessage = e.Message;
            this.LastException = e;
        }

        protected void SetError(Exception ex)
        {
            SetError(ex, false);
        }




        public string GetVersionInfo()
        {
            string res = @".NET Version: " + Environment.Version.ToString() + "\r\n" +
               this.GetType().Assembly.CodeBase;
            return res;
        }
    }
}
