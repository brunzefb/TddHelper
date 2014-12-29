using System;
using log4net.Repository.Hierarchy;

namespace DreamWorks.TddHelper.Utility
{
	internal static class ExceptionLogHelper
	{
		private static readonly log4net.ILog Logger = log4net.LogManager.
			GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void LogException(Exception ex)
		{
			Logger.Warn(string.Format("Time {0} Exception: {1} Runtime termination: {2}", DateTime.Now, ex.Message, e.IsTerminating));
			if (!string.IsNullOrEmpty(ex.StackTrace))
				Logger.Warn(ex.StackTrace);
			if (ex.InnerException != null)
			{
				var ex2 = ex.InnerException as Exception;
				Logger.Warn(string.Format("Time {0} Exception: {1} ", DateTime.Now, ex2.Message));
				if (!string.IsNullOrEmpty(ex2.StackTrace))
					Logger.Warn(ex2.StackTrace);
			}
		}
	}
}