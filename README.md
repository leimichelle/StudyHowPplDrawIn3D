# StudyHowPplDrawIn3D
1. Need to download ViconDataStreamSDK separately
2. Use Windows x86(aka 32bit) CPP sdk
3. Store ViconDLL folder under a directory of your choice
4. Open the ViconDLL project in Visual Studio 2015
5. Solution Explorer -> right clicktVicon_DLL (Universal Windows) -> Properties
	5.1 - Configuration Properties -> VC++ Directories -> Add the path to the ViconDataStreamSDK (likely to be something like $somepath\Vicon\DataStream SDK\Win32\CPP) to Library Directories)
	5.2 - Configuration Properties -> C/C++ -> General -> Similar to 5.1, add the path to Additional Include Directories
6. Rebuild the ViconDLL.dll
7. Do the following only once after you first build the 3D drawing application in Unity
	7.1 - Open the built solution in Visual Studio 2017
	7.2 -  Solution Explorer -> right click SketchWithMoCap (Universal Windows) -> Add -> Existing Item
			7.2.1 - Go to the Windows x86(aka 32bit) CPP sdk directory
			7.2.2 - Select all the boost_*.dll and ViconDataStreamSDK_CPP.dll 
			7.2.3 - Add
	7.3 -  Solution Explorer -> right clickt SketchWithMoCap (Universal Windows) -> Add -> Existing Item
			7.3.1 - Go to the directory where you just built you Vicon_DLL.dll
			7.3.2 - Select ViconDLL.dll
			7.3.3 - Add As Link
	7.4 - Under Solution Explorer -> SketchWithMoCap (Universal Windows), select all the dlls that you just added from 7.2 to 7.3, 
			you will see the "Properties" window under the Solution Explorer. Configure "Copy to Output Direction" to "Copy if newer"
8. Save the solution
			