using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class ProcessTest : MonoBehaviour
{
    private Text label;
    // Start is called before the first frame update
   void Start() {
       //ラベルの取得
       this.label = this.GetComponent<Text>();

       //シェルスクリプトの実行
       string path = Application.streamingAssetsPath + "/helloworld.sh";
       Process process = new Process();
       process.StartInfo.FileName = "/bin/bash";
       process.StartInfo.Arguments = "-c \"" + path + "\"";
       process.StartInfo.UseShellExecute = false;
       process.StartInfo.RedirectStandardOutput = true;
       process.StartInfo.CreateNoWindow = true;
       process.Start();

       //結果待ち
       var output = process.StandardOutput.ReadToEnd();
       process.WaitForExit();
       process.Close();

       //ラベルに表示
       this.label.text = output;
   }
}
