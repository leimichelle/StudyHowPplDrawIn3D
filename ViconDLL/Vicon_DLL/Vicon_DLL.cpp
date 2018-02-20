#include "pch.h"
#include "Vicon_DLL.h"

using namespace ViconDataStreamSDK::CPP;
Client *viconClient;
int count;
//CallBackTimer timer;

extern "C" __declspec(dllexport) int StartVicon()
{
	int count = 0;
	std::string serverIpRaw = "172.21.12.103";
	viconClient = new Client();

	Result::Enum connectionResult = viconClient->Connect(serverIpRaw).Result;

	if (connectionResult == Result::Success) {
		viconClient->EnableSegmentData();
		viconClient->EnableMarkerData();
		viconClient->EnableCentroidData();

		viconClient->SetStreamMode(ViconDataStreamSDK::CPP::StreamMode::ClientPullPreFetch);

		//viconClient->SetAxisMapping(Direction::Forward, Direction::Up, Direction::Right);
		//viconClient->SetAxisMapping(Direction::Forward, Direction::Left, Direction::Up);
		//viconClient->SetAxisMapping(Direction::Forward, Direction::Up, Direction::Right);
		viconClient->SetAxisMapping(Direction::Left, Direction::Up, Direction::Forward);
		return 0;
	}
	else {
		return 1;
	}

	//timer.start(20, fetchNewViconFrame);
}

extern "C" __declspec(dllexport) PenData FetchNewViconFrame()
{
	PenData pd;
	pd.valid = -1;
	//qDebug() << "Trying to fetch frame...";
	Result::Enum r;
	r = viconClient->GetFrame().Result;

	pd.valid = r;
	if (r != Result::Success)
	{
		return pd;
	}
	
	//pd.valid = 1;
	//qDebug() << "Frame fetched! Processing...";

	unsigned int SubjectCount = viconClient->GetSubjectCount().SubjectCount;

	for (unsigned int SubjectIndex = 0; SubjectIndex < SubjectCount; ++SubjectIndex)
	{
		std::string SubjectName = viconClient->GetSubjectName(SubjectIndex).SubjectName;

		if (SubjectName != "Pen_experimental" && SubjectName != "HoloLens_experimental")
			continue;

		// Count the number of segments
		unsigned int SegmentCount = viconClient->GetSegmentCount(SubjectName).SegmentCount;

		for (unsigned int SegmentIndex = 0; SegmentIndex < SegmentCount; ++SegmentIndex)
		{
			std::string SegmentName = viconClient->GetSegmentName(SubjectName, SegmentIndex).SegmentName;

			if (SegmentName == "Pen_experimental")
			{

				// Get the global segment translation
				Output_GetSegmentGlobalTranslation penTranslation =
					viconClient->GetSegmentGlobalTranslation(SubjectName, SegmentName);
				pd.x = penTranslation.Translation[0] / 1000;
				pd.y = penTranslation.Translation[1] / 1000;
				pd.z = penTranslation.Translation[2] / 1000;

				//penPos = QVector3D(penTranslation.Translation[0], penTranslation.Translation[1], penTranslation.Translation[2]);
				//penPos /= 1000;		// vicon uses millimetres

									//Get the global segment rotation
				Output_GetSegmentGlobalRotationQuaternion penRotation =
					viconClient->GetSegmentGlobalRotationQuaternion(SubjectName, SegmentName);

				pd.qx = penRotation.Rotation[0];
				pd.qy = penRotation.Rotation[1];
				pd.qz = penRotation.Rotation[2];
				pd.qw = penRotation.Rotation[3];
				//penRot = QQuaternion(penRotation.Rotation[3], penRotation.Rotation[0], penRotation.Rotation[1], penRotation.Rotation[2]);

			}
			else if (SegmentName == "HoloLens_experimental")
			{
				// Get the global segment translation
				Output_GetSegmentGlobalTranslation hololensTranslation =
					viconClient->GetSegmentGlobalTranslation(SubjectName, SegmentName);

				//hololensPos = QVector3D(hololensTranslation.Translation[0], hololensTranslation.Translation[1], hololensTranslation.Translation[2]);
				//hololensPos /= 1000;		// vicon uses millimetres

											//Get the global segment rotation
				Output_GetSegmentGlobalRotationQuaternion hololensRotation =
					viconClient->GetSegmentGlobalRotationQuaternion(SubjectName, SegmentName);

				//hololensRot = QQuaternion(hololensRotation.Rotation[3], hololensRotation.Rotation[0], hololensRotation.Rotation[1], hololensRotation.Rotation[2]);

			}
		}

		// Count the number of markers
		unsigned int MarkerCount = viconClient->GetMarkerCount(SubjectName).MarkerCount;

		if (MarkerCount < 3)
			continue;

		for (unsigned int MarkerIndex = 0; MarkerIndex < MarkerCount; ++MarkerIndex)
		{
			// Get the marker name
			std::string MarkerName = viconClient->GetMarkerName(SubjectName, MarkerIndex).MarkerName;
			// Get the global marker translation
			Output_GetMarkerGlobalTranslation markerTranslation =
				viconClient->GetMarkerGlobalTranslation(SubjectName, MarkerName);

			if (MarkerName.find("Pen") != std::string::npos)	// pen marker names: 0, 1, 2
			{
				//penMarkerPos[MarkerName.back() - '1'] = QVector3D(markerTranslation.Translation[0], markerTranslation.Translation[1], markerTranslation.Translation[2]);
				//penMarkerPos[MarkerName.back() - '1'] /= 1000;		//vicon uses millimetres
			}
			else	// hololens marker names: HoloLens1, HoloLens2, HoloLens3
			{
				//hololensMarkerPos[MarkerName.back() - '1'] = QVector3D(markerTranslation.Translation[0], markerTranslation.Translation[1], markerTranslation.Translation[2]);
				//hololensMarkerPos[MarkerName.back() - '1'] /= 1000;
			}
		}
	}
	return pd;
}

