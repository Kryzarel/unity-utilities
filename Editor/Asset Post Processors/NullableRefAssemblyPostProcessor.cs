using UnityEngine;
using UnityEditor;
using System.Xml.Linq;
using System.Linq;
using UnityEditor.Compilation;
using System;
using System.IO;

public class NullableRefAssemblyPostProcessor : AssetPostprocessor
{
	private static readonly bool log = false;

	public static string OnGeneratedCSProject(string path, string content)
	{
		XDocument doc = XDocument.Parse(content);
		XNamespace xNamespace = doc.Root.Name.Namespace;

		// Use the element's LocalName to bypass namespace. csproj tags are case insensitive, so use StringComparison.OrdinalIgnoreCase
		XElement? assemblyName = doc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("AssemblyName", StringComparison.OrdinalIgnoreCase));

		if (assemblyName == null || string.IsNullOrEmpty(assemblyName.Value))
		{
			Debug.LogError("Couldn't find assembly name for " + path);
			return content;
		}

		// Presumably the assembly name is case sensitive, but we shouldn't care about culture, so use StringComparison.Ordinal
		Assembly? assembly = CompilationPipeline.GetAssemblies().FirstOrDefault(a => a.name.Equals(assemblyName.Value, StringComparison.Ordinal));

		if (assembly == null)
		{
			Debug.LogError("Couldn't find assembly for " + assemblyName.Value);
			return content;
		}

		bool isNullableRefEnabled = false;
		string reasonKey = string.Empty;
		string reasonValue = string.Empty;

		string[] compilerArguments = assembly.compilerOptions.AdditionalCompilerArguments;

		// Compiler arguments are case insensitive, so use StringComparison.OrdinalIgnoreCase
		isNullableRefEnabled = Array.Exists(compilerArguments, arg => arg.Contains("nullable:enable", StringComparison.OrdinalIgnoreCase));

		if (log && isNullableRefEnabled)
		{
			reasonKey = "Compiler Arguments";
			reasonValue = string.Join(", ", compilerArguments);
		}

		string[] responseFilePaths = assembly.compilerOptions.ResponseFiles;

		for (int i = 0; i < responseFilePaths.Length && !isNullableRefEnabled; i++)
		{
			string rspPath = responseFilePaths[i];
			string rsp = File.ReadAllText(rspPath);
			// Compiler arguments are case insensitive, so use StringComparison.OrdinalIgnoreCase
			isNullableRefEnabled = rsp.Contains("nullable:enable", StringComparison.OrdinalIgnoreCase);

			if (log && isNullableRefEnabled)
			{
				reasonKey = "Response File";
				reasonValue = rspPath + " -> " + rsp;
			}
		}

		if (log)
		{
			if (isNullableRefEnabled)
			{
				Debug.Log($"Nullable Ref ENABLED for assembly: {assembly.name}\nReason: {reasonKey} -> {reasonValue}");
			}
			else
			{
				Debug.Log($"Nullable Ref DISABLED for assembly: {assembly.name}");
			}
		}

		if (isNullableRefEnabled)
		{
			XElement propertyGroup = new(xNamespace + "PropertyGroup");
			XElement nullable = new(xNamespace + "Nullable", "enable");
			propertyGroup.Add(nullable);
			doc.Root.Add(propertyGroup);

			return doc.ToString();
		}

		return content;
	}
}