using System.Collections.Generic;

namespace DreamWorks.TddHelper.Model
{
	public interface IProjectModel
	{
		void UpdateSolutionId();
		string ImplementationProjectFromTestProject(string sourceProject);
		string TestProjectFromImplementationProject(string sourceProject);
		List<string> ProjectPathsList { get; }
		List<string> CsharpFilesInProject { get; }
		void AddFileAssociationToCache(string implementation, string test);
		void AddProjectAssociationToCache(string implementation, string test);
		string FindTargetFileInCache(string targetFileName);
		string ProjectPathFromFilePath(string path);
		void GetCSharpFilesFromSolution();
	}
}