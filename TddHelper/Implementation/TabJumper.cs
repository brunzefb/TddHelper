﻿// Copyright AB SCIEX 2014. All rights reserved.

using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;

namespace DreamWorks.TddHelper.Implementation
{
	internal class TabJumper
	{
		private const string Document = "Document";
		private readonly DTE2 _dte;
		
		internal TabJumper(DTE2 dte)
		{
			_dte = dte;
		}

		internal void JumpRight(object sender, EventArgs e)
		{
			ExecuteJump(true);
		}


		internal void JumpLeft(object sender, EventArgs e)
		{
			ExecuteJump(false);
		}

		internal void ExecuteJump(bool jumpRight)
		{
			
			var topLevelWindows = GetSortedTopLevelWindows();
			if (topLevelWindows.Count < 2)
				return;
			var activeIndex = FindActiveWindowIndex(topLevelWindows);
			if (jumpRight)
				activeIndex--;
			else
				activeIndex++;
			activeIndex = (activeIndex < 0 ? activeIndex + topLevelWindows.Count : activeIndex) % topLevelWindows.Count;
			topLevelWindows[activeIndex].Activate();
		}

		internal int FindActiveWindowIndex(IReadOnlyList<Window> topLevelWindows)
		{
			for (var i = 0; i < topLevelWindows.Count; ++i)
			{
				if (topLevelWindows[i].Document != _dte.ActiveDocument)
					continue;
				return i;
			}
			return 0;
		}

		internal List<Window> GetSortedTopLevelWindows()
		{
			// Documents with a "left" or "top" value > 0 are the focused ones in each group, 
			// so we only need to collect those
			var topLevelWindows = new List<Window>();
			foreach (Window window in _dte.Windows)
			{

				if (window.Kind == Document && (window.Left > 0 || window.Top > 0))
					topLevelWindows.Add(window);
			}
			topLevelWindows.Sort((a, b) => a.Left < b.Left ? -1 : 1);
			return topLevelWindows;
		}

	}
}