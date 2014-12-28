using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using SnkHelper;
using VSLangProj;
using VSLangProj80;

namespace DreamWorks.TddHelper.Implementation
{
	public static class CreateProjectHelper
	{
		private const string ClassLibraryProjectTemplateName = "ClassLibrary.zip";
		private const string Class1ItemCreatedByTemplate = "Class1.cs";
		private const string CSharpLanguageName = "CSharp";
		private const int KeyLineLength = 80;
		private const string SignAssemblyPropertyName = "SignAssembly";
		private const string AssemblyOriginatorKeyFilePropertyName = "AssemblyOriginatorKeyFile";
		private const string AssemblyInfoCsFile = "AssemblyInfo.cs";

		public static bool CreateProject(string projectName)
		{
			var solution = Access.Dte.Solution as Solution2;
			if (solution == null || string.IsNullOrEmpty(solution.FileName))
				return false;
			var directoryName = Path.GetDirectoryName(solution.FileName);
			if (directoryName == null)
				return false;
			var targetDir = Path.Combine(directoryName, projectName);
			if (!targetDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
				targetDir += Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			if (Directory.Exists(targetDir))
				return false;
			string newProjectName = projectName;
			var projectItemTemplate = solution.GetProjectTemplate(ClassLibraryProjectTemplateName, CSharpLanguageName);
			solution.AddFromTemplate(projectItemTemplate, targetDir, newProjectName, false);
			PostCreateProject(newProjectName);
			return true;
		}

		private static void PostCreateProject(string newProjectName)
		{
			Project newlyCreatedProject = null;
			foreach (Project project in Access.Dte.Solution.Projects)
			{
				if (project.Name != newProjectName)
					continue;
				newlyCreatedProject = project;
				break;
			}
			if (newlyCreatedProject == null)
				return;

			AddProjectAssociationToCache(newlyCreatedProject);
			RemoveClass1File(newlyCreatedProject);
			AddProjectReferenceToImplementationProject(newlyCreatedProject);
		}

		private static void AddProjectAssociationToCache(Project newlyCreated)
		{
			var newlyCreatedTargetProjectPath = newlyCreated.FullName;
			var sourceProjectPath = GetSourceProjectPath();

			if (SourceTargetInfo.IsSourcePathTest)
			{
				Access.ProjectModel.AddProjectAssociationToCache(newlyCreatedTargetProjectPath,
					sourceProjectPath);
			}
			else
			{
				Access.ProjectModel.AddProjectAssociationToCache(sourceProjectPath,
					newlyCreatedTargetProjectPath);
			}
		}

		private static void AddProjectReferenceToImplementationProject(Project newlyCreatedProject)
		{
			if (!StaticOptions.MainOptions.CreateReference)
				return;
			var vsProjectNewlyCreated = newlyCreatedProject.Object as VSProject2;
			if (vsProjectNewlyCreated == null)
				return;
			var newlyCreatedRefs = vsProjectNewlyCreated.References;
			if (newlyCreatedRefs == null)
				return;
			string sourceProjectPath = GetSourceProjectPath();
			var sourceProject = ProjectFromPath(sourceProjectPath);
			if (sourceProject == null)
				return;

			if (SourceTargetInfo.IsSourcePathTest)
			{
				AddImplementationReferenceToTestProject(newlyCreatedProject, sourceProject);
			}
			else
			{
				HandleNewlyCreatedTestProject(newlyCreatedProject, newlyCreatedRefs, sourceProject);
			}
		}

		private static void HandleNewlyCreatedTestProject(Project newlyCreatedProject, References newlyCreatedRefs,
			Project sourceProject)
		{
			// newly created project is test, so we must add
			// a reference to the implementation (source) project
			var testProjectRefs = newlyCreatedRefs;
			var testProject = newlyCreatedProject;
			testProjectRefs.AddProject(sourceProject);
			AddTestFrameworkReferenceWithNuGet(testProject);
			AddTestFramworkReferenceWithFile(testProjectRefs);

			if (!StaticOptions.MainOptions.MakeFriendAssembly)
				return;
			MakeFriendAssembly(sourceProject, testProject);
			MakeNewAssemblyStrongNamed(sourceProject, testProject);
		}

		private static void AddImplementationReferenceToTestProject(Project newlyCreatedProject, 
			Project sourceProject)
		{
			// newly created project is implementation, so add
			// a reference to the newly created to the test *source"
			// project
			var vsTestProject = sourceProject.Object as VSProject2;
			if (vsTestProject == null)
				return;
			var sourceTestProjRefs = vsTestProject.References;
			if (sourceTestProjRefs == null)
				return;
			sourceTestProjRefs.AddProject(newlyCreatedProject);
		}

		private static void AddTestFramworkReferenceWithFile(References testProjectReferences)
		{
			if (StaticOptions.ReferencesOptions.UseFileAssembly &&
			    File.Exists(StaticOptions.ReferencesOptions.AssemblyPath))
				testProjectReferences.Add(StaticOptions.ReferencesOptions.AssemblyPath);
		}

		private static void AddTestFrameworkReferenceWithNuGet(Project testProject)
		{
			if (!StaticOptions.ReferencesOptions.UseNuGet
			    || string.IsNullOrEmpty(StaticOptions.ReferencesOptions.PackageId))
				return;
			int verMajor;
			int verMinor;
			int verBuild;
			if (!int.TryParse(StaticOptions.ReferencesOptions.VersionMajor, out verMajor))
				verMajor = 1;
			int.TryParse(StaticOptions.ReferencesOptions.VersionMinor, out verMinor);
			int.TryParse(StaticOptions.ReferencesOptions.VersionBuild, out verBuild);
			try
			{
				Access.PackageInstaller.InstallPackage(null, testProject,
					StaticOptions.ReferencesOptions.PackageId,
					new Version(verMajor, verMinor, verBuild),
					false);
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch
			{
			}
		}

		private static void MakeNewAssemblyStrongNamed(Project implProject, Project testProject)
		{
			var implementionFullPathToSnk = GetFullPathToSnkFile(implProject);
			if (string.IsNullOrEmpty(implementionFullPathToSnk))
				return;
			var testProjectFolder = Path.GetDirectoryName(testProject.FullName);
			if (string.IsNullOrEmpty(testProjectFolder))
				return;

			var fullTargetPath = Path.Combine(testProjectFolder, Path.GetFileName(implementionFullPathToSnk));
			File.Copy(implementionFullPathToSnk, fullTargetPath, true);

			// make test project signed.
			var testProj = testProject.Object as VSProject2;
			if (testProj == null)
				return;
			var testProjectProps = testProj.Project.Properties;
			if (testProjectProps == null)
				return;
			testProjectProps.Item(SignAssemblyPropertyName).Value = true;
			testProjectProps.Item(AssemblyOriginatorKeyFilePropertyName).Value =
				Path.GetFileName(implementionFullPathToSnk);
			testProject.Save();
		}

		private static void MakeFriendAssembly(Project sourceProject, Project newlyCreatedProject)
		{
			// the way to get the sign info out of the project is not very well documented...
			var fullPathToSnk = GetFullPathToSnkFile(sourceProject);
			if (string.IsNullOrEmpty(fullPathToSnk))
				return;
			var publicKeyAsString = Helper.PublicKeyFromSnkFile(fullPathToSnk);
			if (string.IsNullOrEmpty(publicKeyAsString))
				return;
			string assemblyName = newlyCreatedProject.Name;
			if (string.IsNullOrEmpty(publicKeyAsString))
				return;
			var patchString = GetInternalsVisibleToString(assemblyName, publicKeyAsString);
			PatchSourceAssemblyInfoCsWithInternalsVisibleTo(sourceProject, patchString);
		}

		private static void PatchSourceAssemblyInfoCsWithInternalsVisibleTo(Project sourceProject, string patchString)
		{
			string fullPathToAssemblyInfoCs = string.Empty;
			foreach (var fullPathToFile in Access.ProjectModel.CsharpFilesInProject)
			{
				var fileName = Path.GetFileName(fullPathToFile);
				if (String.Equals(fileName, AssemblyInfoCsFile, StringComparison.OrdinalIgnoreCase))
				{
					if (File.Exists(fullPathToFile) &&
					    Access.ProjectModel.ProjectPathFromFilePath(fullPathToFile) == sourceProject.FullName)
					{
						fullPathToAssemblyInfoCs = fullPathToFile;
						break;
					}
				}
			}
			if (string.IsNullOrEmpty(fullPathToAssemblyInfoCs))
				return;
			using (var sw = File.AppendText(fullPathToAssemblyInfoCs))
			{
				sw.WriteLine("\r\n");
				sw.WriteLine(patchString);
			}
		}

		private static string GetFullPathToSnkFile(Project sourceProject)
		{
			var sourceProj = sourceProject.Object as VSProject2;
			if (sourceProj == null)
				return null;
			var props = sourceProj.Project.Properties;
			if (props == null)
				return null;
			var isSigned = props.Item(SignAssemblyPropertyName).Value as bool?;
			var keyFile = props.Item(AssemblyOriginatorKeyFilePropertyName).Value as string;
			var projectDirectoryName = Path.GetDirectoryName(sourceProject.FullName);
			if (string.IsNullOrEmpty(projectDirectoryName) || string.IsNullOrEmpty(keyFile))
				return null;
			string fullPathToSnk = Path.Combine(projectDirectoryName, keyFile);
			if (!isSigned.HasValue || !isSigned.Value)
				return null;
			if (!File.Exists(fullPathToSnk))
				return null;
			return fullPathToSnk;
		}

		private static string GetInternalsVisibleToString(string assemblyName, string publicKey)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < publicKey.Length; i += KeyLineLength)
			{
				if (i < 240)
					sb.AppendFormat("\t\t\"{0}\" + \r\n", publicKey.Substring(i, KeyLineLength));
				else
					sb.AppendFormat("\t\t\"{0}\" \r\n", publicKey.Substring(i));
			}
			const string template = "[assembly: InternalsVisibleTo (\r\n" +
			                        "\t\"{0}, PublicKey=\" + \r\n{1}" + ")]";
			return string.Format(template, assemblyName, sb);
		}

		private static void RemoveClass1File(Project newlyCreatedProject)
		{
			ProjectItem projectItem = null;
			if (ContainsItem(Class1ItemCreatedByTemplate, newlyCreatedProject.ProjectItems))
				projectItem = newlyCreatedProject.ProjectItems.Item(Class1ItemCreatedByTemplate);
			if (projectItem != null)
				projectItem.Delete();
		}

		private static bool ContainsItem(string itemName, ProjectItems items)
		{
			return items.Cast<ProjectItem>().Any(item => item.Name == itemName);
		}

		private static Project ProjectFromPath(string path)
		{
			foreach (Project project in Access.Dte.Solution.Projects)
			{
				try
				{
					if (!string.IsNullOrEmpty(project.FullName) &&
					    string.Equals(project.FullName, path, StringComparison.CurrentCultureIgnoreCase))
						return project;
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
				}
			}
			return null;
		}

		private static string GetSourceProjectPath()
		{
			if (!string.IsNullOrEmpty(SourceTargetInfo.SourcePath))
				return Access.ProjectModel.ProjectPathFromFilePath(SourceTargetInfo.SourcePath);
			return string.Empty;
		}
	}
}