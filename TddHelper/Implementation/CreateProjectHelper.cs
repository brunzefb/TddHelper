using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DreamWorks.TddHelper.Utility;
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
		private static readonly log4net.ILog Logger = log4net.LogManager.
			GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static bool CreateProject(string projectName)
		{
			Logger.Info("CreateProjectHelper.CreateProject");
			var solution = Access.Dte.Solution as Solution2;
			if (solution == null || string.IsNullOrEmpty(solution.FileName))
			{
				Logger.Info("CreateProjectHelper.CreateProject, Solution null");
				return false;
			}
			var directoryName = Path.GetDirectoryName(solution.FileName);
			if (directoryName == null)
			{
				Logger.Info("CreateProjectHelper.CreateProject, Solution Dir null");
				return false;
			}
			var targetDir = Path.Combine(directoryName, projectName);
			if (!targetDir.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
				targetDir += Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			if (Directory.Exists(targetDir))
			{
				Logger.InfoFormat("CreateProjectHelper.CreateProject, Returning because dir exists: {0}", targetDir);
				return false;
			}
			string newProjectName = projectName;
			var projectItemTemplate = solution.GetProjectTemplate(ClassLibraryProjectTemplateName, CSharpLanguageName);
			Logger.InfoFormat("CreateProjectHelper.CreateProject, AddFromTemplate, ProjTempl={0}, targetDir={1}, newProjectName={2}",
				projectItemTemplate, targetDir, newProjectName);
			solution.AddFromTemplate(projectItemTemplate, targetDir, newProjectName, false);
			Logger.Info("CreateProjectHelper.CreateProject, After AddFromTemplate");
			
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
			{
				Logger.Info("CreateProjectHelper.PostCreateProject, newlyCreateProject was null");
				return;
			}

			AddProjectAssociationToCache(newlyCreatedProject);
			RemoveClass1File(newlyCreatedProject);
			AddProjectReferenceToImplementationProject(newlyCreatedProject);
		}

		private static void AddProjectAssociationToCache(Project newlyCreated)
		{
			var newlyCreatedTargetProjectPath = newlyCreated.FullName;
			var sourceProjectPath = GetSourceProjectPath();
			Logger.InfoFormat("CreateProjectHelper.AddProjectAssociationToCache, Adding SourceProj={0}, TargetProj={1} to cache",
				newlyCreatedTargetProjectPath, sourceProjectPath);

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
			{
				Logger.Info("CreateProjectHelper.AddProjectReferenceToImplementationProject, returning because StaticOptions.MainOptions.CreateReference false");
				return;
			}
			var vsProjectNewlyCreated = newlyCreatedProject.Object as VSProject2;
			if (vsProjectNewlyCreated == null)
			{
				Logger.Info("CreateProjectHelper.AddProjectReferenceToImplementationProject, vsProjectNewlyCreated null");
				return;
			}
			var newlyCreatedRefs = vsProjectNewlyCreated.References;
			if (newlyCreatedRefs == null)
			{
				Logger.Info("CreateProjectHelper.AddProjectReferenceToImplementationProject, newlyCreatedRefs null");
				return;
			}
			string sourceProjectPath = GetSourceProjectPath();
			var sourceProject = ProjectFromPath(sourceProjectPath);
			if (sourceProject == null)
			{
				Logger.Info("CreateProjectHelper.AddProjectReferenceToImplementationProject, sourceProject null");
				return;
			}

			if (SourceTargetInfo.IsSourcePathTest)
			{
				Logger.Info("CreateProjectHelper.AddProjectReferenceToImplementationProject, sourcePath is a test");
				HandleNewlyCreatedImplementationProject(newlyCreatedProject, sourceProject);
			}
			else
			{
				Logger.Info("CreateProjectHelper.AddProjectReferenceToImplementationProject, sourcePath is implementation");
				HandleNewlyCreatedTestProject(newlyCreatedProject, newlyCreatedRefs, sourceProject);
			}
		}

		private static void HandleNewlyCreatedTestProject(Project newlyCreatedTestProject, References newlyCreatedRefs,
			Project sourceImplementationProject)
		{
			// newly created project is test, so we must add
			// a reference to the implementation (source) project
			var testProjectRefs = newlyCreatedRefs;
			var testProject = newlyCreatedTestProject;
			Logger.Info("CreateProjectHelper.HandleNewlyCreatedTestProject adding project reference to test project");
			testProjectRefs.AddProject(sourceImplementationProject);
			AddTestFrameworkReferenceWithNuGet(testProject);
			AddTestFramworkReferenceWithFile(testProjectRefs);
			Logger.InfoFormat("CreateProjectHelper.HandleNewlyCreatedTestProject, MakeFriendAssemblyFlag={0}", 
				StaticOptions.MainOptions.MakeFriendAssembly);
			if (StaticOptions.MainOptions.MakeFriendAssembly)
			{
				MakeFriendAssemblyNewlyCreatedTest(sourceImplementationProject, testProject );
			}
			MakeTestAssemblyStrongNamed(sourceImplementationProject, testProject);
		}

		private static void HandleNewlyCreatedImplementationProject(Project implementationProject, Project testProject)
		{
			// newly created project is implementation, so add
			// a reference to the newly created to the test *source"
			// project
			var vsTestProject = testProject.Object as VSProject2;
			if (vsTestProject == null)
			{
				Logger.Info("CreateProjectHelper.HandleNewlyCreatedImplementationProject, vsTestProject is null"); 
				return;
			}
			var sourceTestProjRefs = vsTestProject.References;
			if (sourceTestProjRefs == null)
			{
				Logger.Info("CreateProjectHelper.HandleNewlyCreatedImplementationProject, sourceTestProjectRefs is null"); 
				return;
			}
			Logger.Info("CreateProjectHelper.HandleNewlyCreatedImplementationProject, Adding reference"); 

			sourceTestProjRefs.AddProject(implementationProject);
			Logger.InfoFormat("CreateProjectHelper.HandleNewlyCreatedImplementationProject, MakeFriendAssemblyFlag={0}",
				StaticOptions.MainOptions.MakeFriendAssembly);
			if (StaticOptions.MainOptions.MakeFriendAssembly)
			{
				// in this case the newly created assemblyinfo.cs which we must patch later
				// is not the file to project list
				var projectPath = Path.GetDirectoryName(implementationProject.FullName);
				var file = Path.Combine(projectPath, @"Properties\AssemblyInfo.cs");
				Access.ProjectModel.AddFileToProjectAssociation(file, implementationProject.FullName);
				MakeFriendAssemblyNewlyCreatedImplementation(implementationProject, testProject);
			}

			MakeImplementationAssemblyStrongNamed(implementationProject, testProject);
		}

		private static bool MakeFriendAssemblyNewlyCreatedImplementation(Project implementationProject, Project testProject)
		{
			string patchString;
			var fullPathToSnk = GetFullPathToSnkFile(testProject);
			if (!string.IsNullOrEmpty(fullPathToSnk))
			{
				Logger.InfoFormat("CreateProjectHelper., fullPathToSnk={0}", fullPathToSnk);
				var publicKeyAsString = Helper.PublicKeyFromSnkFile(fullPathToSnk);
				if (string.IsNullOrEmpty(publicKeyAsString))
				{
					Logger.Info("CreateProjectHelper. - PublicKeyFromSnkFile (c++) failed");
					return true;
				}
				patchString = GetInternalsVisibleToString(testProject.Name, publicKeyAsString);
			}
			else
				patchString = string.Format("[assembly: InternalsVisibleTo (\"{0}\")]", testProject.Name);
			PatchSourceAssemblyInfoCsWithInternalsVisibleTo(implementationProject, patchString);
			return false;
		}

		private static void AddTestFramworkReferenceWithFile(References testProjectReferences)
		{
			Logger.InfoFormat("CreateProjectHelper.AddTestFramworkReferenceWithFile, UseFileAssemblyFlag={0}",
					StaticOptions.ReferencesOptions.UseFileAssembly); 

			if (StaticOptions.ReferencesOptions.UseFileAssembly &&
			    File.Exists(StaticOptions.ReferencesOptions.AssemblyPath))
			{
				Logger.InfoFormat("CreateProjectHelper.AddTestFramworkReferenceWithFile, Adding File reference:{0}",
					StaticOptions.ReferencesOptions.AssemblyPath); 
				testProjectReferences.Add(StaticOptions.ReferencesOptions.AssemblyPath);
			}
			else
				Logger.Info("CreateProjectHelper.AddTestFramworkReferenceWithFile, could not add file reference!");
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
				Logger.InfoFormat("CreateProjectHelper.AddTestFrameworkReferenceWithNuGet, Installing package with nuget, " +
				                    "testProject={0}, packageId={1}, verMaj={2}, verMin={3}, verBuild={4}",
									testProject.Name, StaticOptions.ReferencesOptions.PackageId,
									verMajor, verMinor, verBuild);
				Access.PackageInstaller.InstallPackage(null, testProject,
					StaticOptions.ReferencesOptions.PackageId,
					new Version(verMajor, verMinor, verBuild),
					false);
			}
			catch (Exception e)
			{
				ExceptionLogHelper.LogException(e);
			}
		}

		private static void MakeImplementationAssemblyStrongNamed(Project implProject, Project testProject)
		{
			var testProjectSnkPath = GetFullPathToSnkFile(testProject);
			if (string.IsNullOrEmpty(testProjectSnkPath))
			{
				Logger.Info("CreateProjectHelper.MakeImplementationAssemblyStrongNamed, testProjectSnkPath is null");
				return;
			}
			var implementationProjectFolder = Path.GetDirectoryName(implProject.FullName);
			if (string.IsNullOrEmpty(implementationProjectFolder))
			{
				Logger.Info("CreateProjectHelper.MakeImplementationAssemblyStrongNamed, implementationProjectFolder is null");
				return;
			}

			var fullTargetPath = Path.Combine(implementationProjectFolder, Path.GetFileName(testProjectSnkPath));
			Logger.InfoFormat("CreateProjectHelper.MakeImplementationAssemblyStrongNamed, Copying key file from {0} to {1}",
				testProjectSnkPath, fullTargetPath);
			File.Copy(testProjectSnkPath, fullTargetPath, true);

			// make new implementation project signed.
			var implProj = implProject.Object as VSProject2;
			if (implProj == null)
			{
				Logger.Info("CreateProjectHelper.MakeImplementationAssemblyStrongNamed, implProj is null");
				return;
			}
			var implProjectProps = implProj.Project.Properties;
			if (implProjectProps == null)
			{
				Logger.Info("CreateProjectHelper.MakeImplementationAssemblyStrongNamed, implProjectProps is null");
				return;
			}
			implProjectProps.Item(SignAssemblyPropertyName).Value = true;
			implProjectProps.Item(AssemblyOriginatorKeyFilePropertyName).Value =
				Path.GetFileName(testProjectSnkPath);
			implProject.Save();
		}

		private static void MakeTestAssemblyStrongNamed(Project implProject, Project testProject)
		{
			var implementionFullPathToSnk = GetFullPathToSnkFile(implProject);
			if (string.IsNullOrEmpty(implementionFullPathToSnk))
			{
				Logger.Info("CreateProjectHelper.MakeTestAssemblyStrongNamed, implementationFullPathToSnk is null");
				return;
			}
			var testProjectFolder = Path.GetDirectoryName(testProject.FullName);
			if (string.IsNullOrEmpty(testProjectFolder))
			{
				Logger.Info("CreateProjectHelper.MakeTestAssemblyStrongNamed, testProjectFolder is null");
				return;
			}

			var fullTargetPath = Path.Combine(testProjectFolder, Path.GetFileName(implementionFullPathToSnk));
			Logger.InfoFormat("CreateProjectHelper.MakeTestAssemblyStrongNamed, Copying key file from {0} to {1}",
				implementionFullPathToSnk, fullTargetPath);
			File.Copy(implementionFullPathToSnk, fullTargetPath, true);

			// make test project signed.
			var testProj = testProject.Object as VSProject2;
			if (testProj == null)
			{
				Logger.Info("CreateProjectHelper.MakeTestAssemblyStrongNamed, testProj is null");
				return;
			}
			var testProjectProps = testProj.Project.Properties;
			if (testProjectProps == null)
			{
				Logger.Info("CreateProjectHelper.MakeTestAssemblyStrongNamed, testProjectProps is null");
				return;
			}
			testProjectProps.Item(SignAssemblyPropertyName).Value = true;
			testProjectProps.Item(AssemblyOriginatorKeyFilePropertyName).Value =
				Path.GetFileName(implementionFullPathToSnk);
			testProject.Save();
		}

		private static void MakeFriendAssemblyNewlyCreatedTest(Project sourceProject, Project newlyCreatedProject)
		{
			// the way to get the sign info out of the project is not very well documented...
			string patchString;
			var fullPathToSnk = GetFullPathToSnkFile(sourceProject);
			if (!string.IsNullOrEmpty(fullPathToSnk))
			{
				Logger.InfoFormat("CreateProjectHelper.MakeFriendAssemblyNewlyCreatedTest, fullPathToSnk={0}", fullPathToSnk);
				var publicKeyAsString = Helper.PublicKeyFromSnkFile(fullPathToSnk);
				if (string.IsNullOrEmpty(publicKeyAsString))
				{
					Logger.Info("CreateProjectHelper.MakeFriendAssemblyNewlyCreatedTest - PublicKeyFromSnkFile (c++) failed");
					return;
				}
				string assemblyName = newlyCreatedProject.Name;
				patchString = GetInternalsVisibleToString(assemblyName, publicKeyAsString);
			}
			else
				patchString = string.Format("[assembly: InternalsVisibleTo (\"{0}\")]", newlyCreatedProject.Name);
			PatchSourceAssemblyInfoCsWithInternalsVisibleTo(sourceProject, patchString);
		}

		private static void PatchSourceAssemblyInfoCsWithInternalsVisibleTo(Project sourceProject, string patchString)
		{
			Logger.InfoFormat("CreateProjectHelper.PatchSourceAssemblyInfoCsWithInternalsVisibleTo, sourceProject={0}", sourceProject.Name);
			Logger.InfoFormat("patchString={0}", patchString);

			string fullPathToAssemblyInfoCs = string.Empty;
			foreach (var fullPathToFile in Access.ProjectModel.CsharpFilesInProject)
			{
				var fileName = Path.GetFileName(fullPathToFile);
				var projectPathFromFilePath = Access.ProjectModel.ProjectPathFromFilePath(fullPathToFile);
				if (String.Equals(fileName, AssemblyInfoCsFile, StringComparison.OrdinalIgnoreCase))
				{
					if (File.Exists(fullPathToFile) &&
						String.Equals(projectPathFromFilePath, sourceProject.FullName, StringComparison.OrdinalIgnoreCase))
					{
						fullPathToAssemblyInfoCs = fullPathToFile;
						break;
					}
				}
			}
			if (string.IsNullOrEmpty(fullPathToAssemblyInfoCs))
			{
				Logger.Info(
					"CreateProjectHelper.PatchSourceAssemblyInfoCsWithInternalsVisibleTo, could not find path to AssemblyInfo.cs");
				return;
			}
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
			{
				Logger.Info("CreateProjectHelper.GetFullPathToSnkFile sourceProj is null");
				return null;
			}
			var props = sourceProj.Project.Properties;
			if (props == null)
			{
				Logger.Info("CreateProjectHelper.GetFullPathToSnkFile props is null"); 
				return null;
			}
			var isSigned = props.Item(SignAssemblyPropertyName).Value as bool?;
			var keyFile = props.Item(AssemblyOriginatorKeyFilePropertyName).Value as string;
			var projectDirectoryName = Path.GetDirectoryName(sourceProject.FullName);
			if (string.IsNullOrEmpty(projectDirectoryName) || string.IsNullOrEmpty(keyFile))
			{
				if (string.IsNullOrEmpty(keyFile))
					Logger.Info("CreateProjectHelper.GetFullPathToSnkFile keyFile is null");
				else
					Logger.Info("CreateProjectHelper.GetFullPathToSnkFile projectDirectory is null");
				return null;
			}
			Logger.InfoFormat("CreateProjectHelper.GetFullPathToSnkFile isSigned={0}, keyFile={1}, projectDirectoryName={2}",
				isSigned, keyFile, projectDirectoryName);
			string fullPathToSnk = Path.Combine(projectDirectoryName, keyFile);
			if (!isSigned.HasValue || !isSigned.Value)
			{
				Logger.Info("CreateProjectHelper.GetFullPathToSnkFile not signed");
				return null;
			}
			if (!File.Exists(fullPathToSnk))
			{
				Logger.Info("CreateProjectHelper.GetFullPathToSnkFile No key file (fullPathToSnk) on disk");
				return null;
			}
			Logger.InfoFormat("CreateProjectHelper.GetFullPathToSnkFile returning fullPathToSnk={0}", fullPathToSnk); 
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
			{
				var fileName = projectItem.get_FileNames(0);
				if(File.Exists(fileName))
				{
					try
					{
						Logger.InfoFormat("CreateProjectHelper.RemoveClass1File removing file={0}", fileName); 
						File.Delete(fileName);
					}
					catch (Exception e)
					{
						ExceptionLogHelper.LogException(e);
					}
				}
				projectItem.Delete();
			}
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
			Logger.InfoFormat("CreateProjectHelper.ProjectFromPath project found for {0}", path);
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