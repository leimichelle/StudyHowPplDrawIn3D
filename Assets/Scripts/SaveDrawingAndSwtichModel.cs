using System.Collections;
using System.Collections.Generic;
using HoloToolkit.UI.Keyboard;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using HoloToolkit.Sharing;
using HoloToolkit.Unity;
#if !UNITY_EDITOR && UNITY_WSA
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

public class SaveDrawingAndSwtichModel : MonoBehaviour, ISpeechHandler {

    public GameObject sketch;
    public ToggleCalibration tgc;
    public string userName = "NoName";
    private enum Mode { Physical, Virtual, Done };
    private Mode mode = Mode.Virtual;
    private string ModelName = "";
    private int drawnModelsIdx = 0;

    private void Start() {
        if (gameObject.transform.childCount > 0) {
            tgc.cur_model = gameObject.transform.GetChild(drawnModelsIdx).gameObject;
        }       
    }
    public void OnSpeechKeywordRecognized(SpeechEventData eventData) {
        if (eventData.RecognizedText == "Next") {
            SwitchDisplayModel();
            SaveDrawing();
        }
    }

    private void SaveDrawing() {
        /*TODO*/
        //Save ALL the points that the user drew
        //Comment out for now. Somehow it has been only successful to generate the 1st file :(
        //WriteStokeDataToFile(userName);
        //Save the sketch into a object file somehow?
        MeshFilter mf = sketch.GetComponent<MeshFilter>();
        if (mf.sharedMesh) {
            mf.sharedMesh.Clear();
        }
    }

    private void SwitchDisplayModel() {
        switch (mode) {
            case Mode.Virtual:
                if (drawnModelsIdx < gameObject.transform.childCount) {
                    ModelName = "virtual_" + gameObject.transform.GetChild(drawnModelsIdx).name;
                    gameObject.transform.GetChild(drawnModelsIdx).gameObject.SetActive(false);
                    drawnModelsIdx++;
                    if (drawnModelsIdx < gameObject.transform.childCount) { 
                        GameObject nextModel = gameObject.transform.GetChild(drawnModelsIdx).gameObject;
                        nextModel.SetActive(true);
                        WorldAnchorManager.Instance.AttachAnchor(nextModel);
                        tgc.cur_model = nextModel;
                    }

                }
                else {
                    drawnModelsIdx = 0;
                    mode = Mode.Physical;
                }
                break;
            case (Mode.Physical):
                if (drawnModelsIdx < gameObject.transform.childCount) {
                    ModelName = "physical_" + gameObject.transform.GetChild(drawnModelsIdx).name;
                    drawnModelsIdx++;
                }
                else {
                    drawnModelsIdx = 0;
                    mode = Mode.Done;
                }
                break;
            case Mode.Done:
                break;
        }
    }

    public string GenerateStringFromPointsData() {
        string s = "";
        Draw draw = sketch.GetComponent<Draw>();
        int idx = 0;
        foreach (int n in draw.ns) {
            s += n.ToString() + "\n";
            for (int i = 0; i < n; i++) {
                s += draw.points[idx].x.ToString() + " " + draw.points[idx].y.ToString() + " " + draw.points[idx].z.ToString() + "\n";
                s += draw.ups[idx].x.ToString() + " " + draw.ups[idx].y.ToString() + " " + draw.ups[idx].z.ToString() + "\n";
                s += draw.timestamps[idx].ToString() + "\n";
                idx++;
            }
        }
        return s;
    }

    public void WriteStokeDataToFile(string immediateFolder) {

#if !UNITY_EDITOR && UNITY_METRO
    string directory = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, immediateFolder);
    if(!Directory.Exists(directory))
    {    
     //if it doesn't, create it
        Directory.CreateDirectory(directory);
 
    }
  using (Stream stream = OpenFileForWrite(directory, ModelName + ".txt")) {
    string s = GenerateStringFromPointsData();
    byte[] data = System.Text.Encoding.ASCII.GetBytes(s);
    stream.Write(data, 0, data.Length);
    stream.Flush();
  }
#endif
    }

    private static Stream OpenFileForWrite(string folderName, string fileName) {
        Stream stream = null;
#if !UNITY_EDITOR && UNITY_METRO
  Task task = new Task(
    async () => {
      StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folderName);
      StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
      stream = await file.OpenStreamForWriteAsync();
    });
  task.Start();
  task.Wait();
#endif
        return stream;
    }
}
