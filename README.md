TddHelper
=========

Introduction
------------
The TddHelper extension is designed to simplify your life if you do Test-Driven Development. It is a helper utility that exposes commands to move to the next tab group, split windows, and jump to test or implementation files (C# only).  If a test or implementation file is not found, the utility can create both test/implementation classes and projects.  For created projects, automatic references to unit testing framework is supported, and friend assemblies (also strong-named ones) are supported. 
Where it is possible, the extension tries to support both jumping from Test -> Implementation and vice-versa.  This also applies to class and project creation – you can create the test project first and test class, strong-name the test project and add your unit-test framework assembly and then create the implementation assembly just by jumping to the non-existing implementation.  When creating test projects, a reference to the test framework is added automatically if so configured.

Compatibility
-------------
Tested on Visual Studio 2013 only for now.  Have not tested with Visual Studio 2015. Probably won’t work with VS 2010, VS2012 (not tested).

Installation
------------
There are three options to install the extension
1.	(Preferred) Installation is through the Visual Studio Gallery. Search for TddHelper. 
2.	Use GitHub .vsix installer. The GitHub project has an Installer folder containing the TddHelper.vsix file.  Installing that file may get you a version that is work in progress.
3.	Get the source code (use a release tag), build it and then run the output TddHelper.vsix file.  You need the Visual Studio Extension SDK (free) and Visual Studio 2013 to build the project.

Support/Feedback
----------------
If you find a bug, please add it to the issue tracker on github. https://github.com/brunzefb/TddHelper. You also can email me at brunzefb_AT_gmail.com, please put TddHelper in the subject line. I will only respond if an issue has been created. The extension generates comprehensive logs info stored in C:\ProgramData\TddHelper\TddHelper.log, so please attach the log file to the issue along with what you were doing at the time.

Documentation
-------------
The project has a .Docx file that explains the extension's usage.  This can be found in (Root)\TddHelper\TDD Helper Getting Started Guide.docx

