using System;
using System.Collections.Generic;
using System.IO;

// Disable warnings about XML documentation
#pragma warning disable 1591


namespace MoonSharp.Interpreter.CoreLib
{
	/// <summary>
	/// Class implementing loading Lua functions like 'require', 'load', etc.
	/// </summary>
	[MoonSharpModule]
	public class LoadModule
	{
		public static void MoonSharpInit(Table globalTable, Table ioTable)
		{
			DynValue package = globalTable.Get("package");

			if (package.IsNil())
			{
				package = DynValue.NewTable(globalTable.OwnerScript);
				globalTable["package"] = package;
			}
			else if (package.Type != DataType.Table)
			{
				throw new InternalErrorException("'package' global variable was found and it is not a table");
			}

#if PCL || ENABLE_DOTNET || NETFX_CORE
			string cfg = "\\\n;\n?\n!\n-\n";
#else
			string cfg = System.IO.Path.DirectorySeparatorChar + "\n;\n?\n!\n-\n";
#endif

			package.Table.Set("config", DynValue.NewString(cfg));
		}



		// load (ld [, source [, mode [, env]]])
		// ----------------------------------------------------------------
		// Loads a chunk.
		//
		// If ld is a string, the chunk is this string.
		//
		// If there are no syntactic errors, returns the compiled chunk as a function;
		// otherwise, returns nil plus the error message.
		//
		// source is used as the source of the chunk for error messages and debug
		// information (see §4.9). When absent, it defaults to ld, if ld is a string,
		// or to "=(load)" otherwise.
		//
		// The string mode is ignored, and assumed to be "t";
		[MoonSharpModuleMethod]
		public static DynValue load(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return load_impl(executionContext, args, null);
		}

		// loadsafe (ld [, source [, mode [, env]]])
		// ----------------------------------------------------------------
		// Same as load, except that "env" defaults to the current environment of the function
		// calling load, instead of the actual global environment.
		[MoonSharpModuleMethod]
		public static DynValue loadsafe(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return load_impl(executionContext, args, GetSafeDefaultEnv(executionContext));
		}

		public static DynValue load_impl(ScriptExecutionContext executionContext, CallbackArguments args, Table defaultEnv)
		{
			try
			{
				Script S = executionContext.GetScript();
				DynValue ld = args[0];
				string script = "";

				if (ld.Type == DataType.Function)
				{
					while (true)
					{
						DynValue ret = executionContext.GetScript().Call(ld);
						if (ret.Type == DataType.String && ret.String.Length > 0)
							script += ret.String;
						else if (ret.IsNil())
							break;
						else
							return DynValue.NewTuple(DynValue.Nil, DynValue.NewString("reader function must return a string"));
					}
				}
				else if (ld.Type == DataType.String)
				{
					script = ld.String;
				}
				else
				{
					args.AsType(0, "load", DataType.Function, false);
				}

				DynValue source = args.AsType(1, "load", DataType.String, true);
				DynValue env = args.AsType(3, "load", DataType.Table, true);

				DynValue fn = S.LoadString(script,
					!env.IsNil() ? env.Table : defaultEnv,
					!source.IsNil() ? source.String : "=(load)");

				return fn;
			}
			catch (SyntaxErrorException ex)
			{
				return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(ex.DecoratedMessage ?? ex.Message));
			}
		}

		// loadfile ([filename [, mode [, env]]])
		// ----------------------------------------------------------------
		// Similar to load, but gets the chunk from file filename or from the standard input,
		// if no file name is given. INCOMPAT: stdin not supported, mode ignored
		[MoonSharpModuleMethod]
		public static DynValue loadfile(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return loadfile_impl(executionContext, args, null);
		}

		// loadfile ([filename [, mode [, env]]])
		// ----------------------------------------------------------------
		// Same as loadfile, except that "env" defaults to the current environment of the function
		// calling load, instead of the actual global environment.
		[MoonSharpModuleMethod]
		public static DynValue loadfilesafe(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return loadfile_impl(executionContext, args, GetSafeDefaultEnv(executionContext));
		}



		private static DynValue loadfile_impl(ScriptExecutionContext executionContext, CallbackArguments args, Table defaultEnv, bool catchError = true)
		{
			try {
				Script S = executionContext.GetScript();
				DynValue v = args.AsType(0, "loadfile", DataType.String, false);
				DynValue env = args.AsType(2, "loadfile", DataType.Table, true);

				string str;
				if (v.String.StartsWith(DataRoot)) str = v.String;
				else                               str = (v.String.Replace("\\", "/").StartsWith("/") ? "" : "/") + v.String;
				string suffix = str.StartsWith(DataRoot) ? DataRoot : "raw";
				ExplorePath(ref str, ref suffix);
				return S.LoadFile(str, env.IsNil() ? defaultEnv : env.Table);
			} catch (SyntaxErrorException ex) {
				return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(ex.DecoratedMessage ?? ex.Message));
			} catch (Exception) {
				if (!catchError)
					return DynValue.Nil;
				throw;
			}
		}


		private static Table GetSafeDefaultEnv(ScriptExecutionContext executionContext)
		{
			Table env = executionContext.CurrentGlobalEnv;

			if (env == null)
				throw new ScriptRuntimeException("current environment cannot be backtracked.");

			return env;
		}

		//dofile ([filename])
		//--------------------------------------------------------------------------------------------------------------
		//Opens the named file and executes its contents as a Lua chunk. When called without arguments,
		//dofile executes the contents of the standard input (stdin). Returns all values returned by the chunk.
		//In case of errors, dofile propagates the error to its caller (that is, dofile does not run in protected mode).
		[MoonSharpModuleMethod]
		public static DynValue dofile(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			try
			{
				Script S = executionContext.GetScript();
				DynValue v = args.AsType(0, "dofile", DataType.String, false);

				string str;
				if (v.String.StartsWith(DataRoot)) str = v.String;
				else                               str = (v.String.Replace("\\", "/").StartsWith("/") ? "" : "/") + v.String;
				string suffix = str.StartsWith(DataRoot) ? DataRoot : "raw";
				ExplorePath(ref str, ref suffix);
				DynValue fn = S.LoadFile(str);

				return DynValue.NewTailCallReq(fn); // tail call to dofile
			}
			catch (SyntaxErrorException ex)
			{
				throw new ScriptRuntimeException(ex);
			}
		}

		//require (modname)
		//----------------------------------------------------------------------------------------------------------------
		//Loads the given module. The function starts by looking into the package.loaded table to determine whether
		//modname is already loaded. If it is, then require returns the value stored at package.loaded[modname].
		//Otherwise, it tries to find a loader for the module.
		//
		//To find a loader, require is guided by the package.loaders array. By changing this array, we can change
		//how require looks for a module. The following explanation is based on the default configuration for package.loaders.
		//
		//First require queries package.preload[modname]. If it has a value, this value (which should be a function)
		//is the loader. Otherwise require searches for a Lua loader using the path stored in package.path.
		//If that also fails, it searches for a C loader using the path stored in package.cpath. If that also fails,
		//it tries an all-in-one loader (see package.loaders).
		//
		//Once a loader is found, require calls the loader with a single argument, modname. If the loader returns any value,
		//require assigns the returned value to package.loaded[modname]. If the loader returns no value and has not assigned
		//any value to package.loaded[modname], then require assigns true to this entry. In any case, require returns the
		//final value of package.loaded[modname].
		//
		//If there is any error loading or running the module, or if it cannot find any loader for the module, then require
		//signals an error.
		[MoonSharpModuleMethod]
		public static DynValue __require_clr_impl(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args.AsType(0, "__require_clr_impl", DataType.String, false);
			string s = v.String.Replace("..", "¤").Replace(".", "/").Replace("¤", "..");

			CallbackArguments newArgs = new CallbackArguments(new List<DynValue> { DynValue.NewString(ModDataPath + "Lua/" + s + ".lua"), args[1], args[2] }, args.IsMethodCall);
			DynValue fn = loadfile_impl(executionContext, newArgs, null, false);
			if (fn.Type != DataType.Nil) return fn; // tail call to dofile

			newArgs = new CallbackArguments(new List<DynValue> { DynValue.NewString(ModDataPath + "Lua/Libraries/" + s + ".lua"), args[1], args[2] }, args.IsMethodCall);
			fn = loadfile_impl(executionContext, newArgs, null);
			return fn; // tail call to dofile
		}


		[MoonSharpModuleMethod]
		public const string require = @"
function(modulename)
	if (package == nil) then package = { }; end
	if (package.loaded == nil) then package.loaded = { }; end

	local m = package.loaded[modulename];

	if (m ~= nil) then
		return m;
	end

	local func = __require_clr_impl(modulename);

	local res = func(modulename);

	if (res == nil) then
		res = true;
	end

	package.loaded[modulename] = res;

	return res;
end";

		public static string DataRoot;
		public static string ModDataPath     { get { return Path.Combine(DataRoot, "Mods" + Path.DirectorySeparatorChar + ModFolder + Path.DirectorySeparatorChar); } }
		public static string DefaultDataPath { get { return Path.Combine(DataRoot, "Default" + Path.DirectorySeparatorChar);                                        } }
		public static string ModFolder;

		/// <summary>
		/// Checks if a file exists in CYF's Default or Mods folder and returns a clean path to it.
		/// </summary>
		/// <param name="fileName">Path to the file to require, relative or absolute. Will also contain the clean path to the existing resource if found.</param>
		/// <param name="pathSuffix">String to add to the tested path to check in the given folder.</param>
		/// <param name="errorOnFailure">Defines whether the error screen should be displayed if the file isn't in either folder.</param>
		/// <param name="needsAbsolutePath">True if you want to get the absolute path to the file, false otherwise.</param>
		/// <param name="needsToExist">True if the file you're looking for needs to exist.</param>
		/// <returns>True if the sanitization was successful, false otherwise.</returns>
		public static bool RequireFile(ref string fileName, string pathSuffix, bool errorOnFailure = true, bool needsAbsolutePath = false, bool needsToExist = true) {
			string baseFileName = fileName;
			string fileNameMod, fileNameDefault;
			// Get the presumed absolute path to the mod and default folder to this resource if it's a relative path
			if (!fileName.Replace('\\', '/').Contains(DataRoot.Replace('\\', '/'))) {
				if (fileName.Replace('\\', '/').StartsWith("/")) {
					fileNameMod = Path.Combine(DataRoot, fileName.TrimStart('/'));
					fileNameDefault = fileNameMod;
				} else {
					fileNameMod = Path.Combine(ModDataPath, Path.Combine(pathSuffix, fileName));
					fileNameDefault = Path.Combine(DefaultDataPath, Path.Combine(pathSuffix, fileName));
				}
			} else {
				fileNameMod = fileName;
				fileNameDefault = fileName;
			}

			// Check if the resource exists using the mod path
			string error;
			try {
				string modPath = pathSuffix;
				ExplorePath(ref fileNameMod, ref modPath);
				// Keep the path to the mod folder in case of failure (used to open nonexistent files!)
				fileName = fileNameMod;
				if (needsToExist && !new FileInfo(fileNameMod).Exists) throw new CYFException("The file " + fileNameMod + " doesn't exist.");

				if (needsAbsolutePath) return true;

				Uri uriRel = new Uri(modPath + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(fileName));
				fileName = Uri.UnescapeDataString(uriRel.OriginalString);
				return true;
			} catch (Exception e) { error = e.Message; }

			// Check if the resource exists using the default path
			try {
				string defaultPath = pathSuffix;
				ExplorePath(ref fileNameDefault, ref defaultPath);
				if (needsToExist && !new FileInfo(fileNameDefault).Exists) throw new CYFException("The file " + fileNameDefault + " doesn't exist.");
				fileName = fileNameDefault;

				if (needsAbsolutePath) return true;

				Uri uriRel = new Uri(defaultPath).MakeRelativeUri(new Uri(fileName));
				fileName = Uri.UnescapeDataString(uriRel.OriginalString);
				return true;
			} catch (Exception e) { error = "Mod path error: " + error + "\n\nDefault path error: " + e.Message; }

			if (errorOnFailure)
				throw new CYFException("Attempted to load " + baseFileName + " from either a mod or default directory, but it was missing in both.\n\n" + error);
			return false;
		}

		/// <summary>
		/// Checks if a file exists in CYF's Default or Mods folder and returns a clean path to it.
		/// </summary>
		/// <param name="fullPath">Path to the file to require.</param>
		/// <param name="pathSuffix">String to add to the tested path to check in the given folder.</param>
		public static void ExplorePath(ref string fullPath, ref string pathSuffix) {
			fullPath = fullPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

			if (pathSuffix == "raw")
				pathSuffix = "/";

			if      (pathSuffix.Contains(DataRoot))      { }
			else if (fullPath.Contains(ModDataPath))     pathSuffix = Path.Combine(ModDataPath,     pathSuffix);
			else if (fullPath.Contains(DefaultDataPath)) pathSuffix = Path.Combine(DefaultDataPath, pathSuffix);
			else if (fullPath.Contains(DataRoot))        pathSuffix = Path.Combine(DataRoot,        pathSuffix);
			// Fetch CYF's root folder if none has been found (Used for dofile, require, loadfile...)
			else {
				pathSuffix = fullPath.StartsWith(Path.DirectorySeparatorChar.ToString()) ? DataRoot : Path.Combine(ModDataPath, pathSuffix);
				fullPath = Path.Combine(pathSuffix, fullPath.TrimStart(Path.DirectorySeparatorChar));
			}

			fullPath = fullPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

			// Get the folder containing the resource to load
			string fileName = fullPath.Substring(fullPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
			DirectoryInfo endFolder = new DirectoryInfo(fullPath.Substring(0, fullPath.LastIndexOf(Path.DirectorySeparatorChar)));
			if (!endFolder.Exists)
				throw new CYFException("The path \"" + endFolder.FullName + "\" (file is \"" + fullPath + "\") doesn't exist.");

			// Check if the final directory is a child of CYF's root directory
			if (!endFolder.FullName.StartsWith(Path.Combine(DataRoot, "Mods")) && !endFolder.FullName.StartsWith(Path.Combine(DataRoot, "Default")))
				throw new CYFException("The folder \"" + endFolder.FullName + "\" isn't inside of CYF's allowed folders (CYF's path is \"" + DataRoot + "\"). Please only fetch files from inside CYF's Mods or Default folders.");

			fullPath = endFolder.FullName;
			if (!fullPath.EndsWith(Path.DirectorySeparatorChar.ToString())) fullPath += Path.DirectorySeparatorChar;
			if (!pathSuffix.EndsWith(Path.DirectorySeparatorChar.ToString())) pathSuffix += Path.DirectorySeparatorChar;

			fullPath = Path.Combine(fullPath, fileName).Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
		}
	}

	public class CYFException : ScriptRuntimeException {
		public CYFException(string message) : base(message) { }
	}
}
