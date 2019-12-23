using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using UnityEngine.EventSystems;

  
public class readtext : MonoBehaviour
{
	
	int HangZiShu = 0;//每行多少字
	int YeHangShu = 0;//每页多少行

	int CurrentZhangJie = 0;//当前章节数
	int CurrentYeShu = 0;//当前章节数的页数

	public Text text;//UI text,可以将读取的txt在UI中显示出现
	string book = "";//用来存从txt中获取的string

	void Start ()
	{  
		Init ();
		//PlayerPrefs.DeleteAll ();
		CurrentZhangJie = PlayerPrefs.GetInt ("CurrentZhangJie");//获取存储的章节数
		CurrentYeShu = PlayerPrefs.GetInt ("CurrentYeShu");//获取页数
//		CreatTxt ("dpcq");
		FanZhang ();//翻到该章
		FanYe ();//翻到该页
	}

	void Init ()
	{
		int screenW = 1080;
		int screenH = 1920;
	
		//获取每一行能显示多少字
		HangZiShu = (int)(screenW - (Math.Abs (text.rectTransform.offsetMax.x) + Math.Abs (text.rectTransform.offsetMin.x))) / text.fontSize;
		//获取每一页能显示多少行
		YeHangShu = (int)((screenH - (Math.Abs (text.rectTransform.offsetMax.y) + Math.Abs (text.rectTransform.offsetMin.y))) / text.fontSize / 1.1f);
	}

	void Update ()
	{
		if (Input.GetMouseButtonUp (0)) {
			if (Input.mousePosition.y / Screen.height < 0.8f) {
				if (Input.mousePosition.x / Screen.width < 0.6f && Input.mousePosition.x / Screen.width > 0.4f) {
					//这个是给点击中间预留的菜单栏呼出
					OpenCloseOtherObj (!IsOpen);
				} else {
					if (Input.mousePosition.x / Screen.width > 0.666f) {
						CurrentYeShu++;
					} else {
						CurrentYeShu--;
					}
					if (CurrentYeShu < 0) {
						//上一章最后一页
						if (CurrentZhangJie == 0) {
							CurrentYeShu = 0;
							return;
						} else {
							CurrentZhangJie--;
							FanZhang ();
							CurrentYeShu = TempYeNum;
							FanYe ();
						}
					} else if (TempYe [CurrentYeShu] == "") {
						//下一章第一页
						CurrentZhangJie++;
						FanZhang ();
						CurrentYeShu = 0;
						FanYe ();
					} else {
						//本章翻页
						FanYe ();
					}
				}
			}
		}
	}

    public void ReadNext(){
        CurrentYeShu++;
        if (CurrentYeShu < 0)
        {
            //上一章最后一页
            if (CurrentZhangJie == 0)
            {
                CurrentYeShu = 0;
                return;
            }
            else
            {
                CurrentZhangJie--;
                FanZhang();
                CurrentYeShu = TempYeNum;
                FanYe();
            }
        }
        else if (TempYe[CurrentYeShu] == "")
        {
            //下一章第一页
            CurrentZhangJie++;
            FanZhang();
            CurrentYeShu = 0;
            FanYe();
        }
        else
        {
            //本章翻页
            FanYe();
        }
    }

	void FanYe ()
	{
		//翻页对text进行赋值，然后保存一下
		text.text = TempYe [CurrentYeShu];
		PlayerPrefs.SetInt ("CurrentYeShu", CurrentYeShu);
	}

	//使用20个页来显示一章，每一个TempYe就是用来储存一页的内容
	string[] TempYe = new string[40];
	int TempYeNum = 0;
	int TempYeLine = 0;
	void FanZhang ()
	{
		ClearTempYe ();//清空TempYe数组
		var fileAddress = System.IO.Path.Combine (Application.streamingAssetsPath, "sw" + "/第" + (CurrentZhangJie + 1) + "章.txt");  
		FileInfo fInfo0 = new FileInfo (fileAddress);  
		string s = "";  
		if (fInfo0.Exists) {  
			StreamReader r = new StreamReader (fileAddress);  
			book = r.ReadToEnd (); 
		}
		string[] SplitStr = { "\n" };

		//以行分割一章内容
		string[] Line = book.Split (SplitStr, System.StringSplitOptions.None);
		TempYeNum = 0;
		TempYeLine = 0;
		//遍历每段
		for (int i = 0; i < Line.Length; i++) {
			//获取这一段可以在显示端分为多少行
			int lineih = (int)Math.Ceiling ((double)Line [i].Length / HangZiShu);

			for (int j = 0; j < lineih; j++) {
				//逐行赋值，该页一共能赋值HangZiShu行，HangZiShu满后赋值下一个TempYe
				if (j < lineih - 1)
					TempYe [TempYeNum] += Line [i].Substring (j * HangZiShu, HangZiShu);
				else
					TempYe [TempYeNum] += Line [i].Substring (j * HangZiShu);
				TempYeLine++;
				if (TempYeLine >= YeHangShu-1) {
					TempYeNum++;
					TempYeLine = 0;
				}
			}
			//当不为该页首行的时候每段要分段
			if (TempYeLine != 0)
				TempYe [TempYeNum] += "\r\n";
		}

		PlayerPrefs.SetInt ("CurrentZhangJie", CurrentZhangJie);

	}

	//清理页资源
	void ClearTempYe ()
	{
		for (int i = 0; i < TempYe.Length; i++) {
			TempYe [i] = "";
		}
	}


		



	public InputField InputField;
	public GameObject[] OtherObj;
	bool IsOpen = false;

	public void GoToZhangJie ()
	{
		Debug.Log (InputField.text);
		CurrentZhangJie = Convert.ToInt32 (InputField.text) - 1;
		FanZhang ();
		CurrentYeShu = 0;
		FanYe ();
		OpenCloseOtherObj (false);
	}

	void OpenCloseOtherObj (bool isopen)
	{
		IsOpen = isopen;
		foreach (GameObject gm in OtherObj) {
			gm.SetActive (IsOpen);
		}
	}

	void CreatTxt (string BookName)
	{
		string BookUrl = Application.streamingAssetsPath + "/" + BookName;//弄一个文件夹，用来存放一千多个章节的txt
		if (!Directory.Exists (BookUrl)) {//判断该路径是否存在，如果没有就创建路径，如果有就说明已经创建过啦
			Directory.CreateDirectory (BookUrl);//创建路径
			//判读需要拆解的txt是否存在，如果有就ReadToEnd，获取整本书txt全部的string
			var fileAddress = System.IO.Path.Combine (Application.streamingAssetsPath, BookName + ".txt");  
			FileInfo fInfo0 = new FileInfo (fileAddress);  
			string s = "";  
			if (fInfo0.Exists) {  
				StreamReader r = new StreamReader (fileAddress);  
				book = r.ReadToEnd (); 
			}  

			//这里是文本txt的分隔符，就是两个回车键，这是我下载的斗破苍穹的分隔符，至于其他书籍是否是用这个，还需要你们自己调研啦
			string[] SplitStr = { "\r\n\r\n" };
			string[] ZhangJie = book.Split (SplitStr, System.StringSplitOptions.None);//把整个string拆解成一个个string最终变成数组
			int num = 0;//用于记录章节数
			foreach (string ZhangJieNeiRong in ZhangJie) {
				if (ZhangJieNeiRong.Length > 120) {//实际过程中会出现空章节，用一个长度来过滤掉太短的章节，当然也不一定要是120，可以根据需要设置
					num++;
					string ZhangJieUrl = BookUrl + "/第" + num + "章.txt";//给每一章一个名字吧，方便加载
					if (!File.Exists (ZhangJieUrl)) {//把分别把每一章写进去创建成txt
						FileStream fs1 = new FileStream (ZhangJieUrl, FileMode.Create, FileAccess.Write);//创建写入文件 
						StreamWriter sw = new StreamWriter (fs1);
						sw.WriteLine (ZhangJieNeiRong);//开始写入值
						sw.Close ();
						fs1.Close ();
					}
				}
			}
		}
	}

} 