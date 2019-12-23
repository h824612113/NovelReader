using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(GetHttpInfo());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public Text text;

    int Zhangjie_num = 0;//实际跟Index相加的章节数
    int Index_num=12533506;//第一章的index
    int ShowZhangjie_num = 0;//用来查询和命名txt的章节数
    IEnumerator GetHttpInfo () {

        //www获取资源，要求网页的index必须是按整数+1，大多数网站都是这样的，别担心
        WWW url = new WWW ("http://www.siluke.com/0/11/11477/"+(Index_num+Zhangjie_num)+".html");

        yield return url;

        byte[] byt = url.bytes;
        //过小的为空章节（思路客大章节之间用空章节隔开，此处用两个变量自加区分开，）
        if (byt.Length < 200)
        {
            Zhangjie_num++;
 
            if (Zhangjie_num < 582)
            {
                StartCoroutine(GetHttpInfo());
            }
        }
        else
        {
            //将byte转为utf8格式
            byt = Encoding.Convert(Encoding.GetEncoding("GBK"), Encoding.UTF8, byt);
            string str = Encoding.UTF8.GetString(byt);
            Debug.Log(str);

            //下面的操作是针对实际文本中筛选获取需要的文本
            //仔细阅读上面debug出来的string，找到想要的规则，尽量找网页之间共性的，单个网页内重复性低的关键字符串
            //思路客的规则大概是正文"&nbsp;&nbsp;&nbsp;&nbsp;"开头，"<script>read_link_up()"结尾
            string str_start = "&nbsp;&nbsp;&nbsp;&nbsp;";
            string str_end = "<script>read_link_up()";
            int IndexofA = str.IndexOf(str_start);
            int IndexofB = str.LastIndexOf(str_end);
            //从str_start开始取，要把自己的字符长度算进去，到str_end的头减去前面的部分，再往前推，所以每本书这里的数值要自己改，如果此处有更好更通用的算法欢迎评论
            string finalstr = str.Substring(IndexofA + str_start.Length, IndexofB - IndexofA - 32);
            //下面就是把正文中一些垃圾字符拿移除，设置好文本间的换行
            finalstr = finalstr.Replace("&nbsp;&nbsp;&nbsp;&nbsp;", "\n");
            finalstr = finalstr.Replace("&nbsp;", "");
            finalstr = finalstr.Replace("<br /><br />", "");

            //Debug.Log((str.Substring(400)));
            //Debug.Log(finalstr);


            //这里作者获取了一下文章的标题，放入整个string的最前面，原理参照如上
            string str_start_title = "title";
            string str_end_title = "/title";
            int IndexofA_title = str.IndexOf(str_start_title);
            int IndexofB_title = str.IndexOf(str_end_title);
            string finalstr_title = str.Substring(IndexofA_title + str_start_title.Length + 1, IndexofB_title - IndexofA_title - 14);
            //Debug.Log(finalstr_title);
            finalstr = finalstr_title + "\n" + finalstr;

            //此处用来显示string拿来debug看看
            text.text = finalstr;

            //接下来就跟之前一样了，写入文件，新建个文件夹分章存，参考斗破，此处没写文件夹新建逻辑，自己去弄文件夹吧，或者你自己写判断创建文件夹
            string BookName = "sw";
            string BookUrl = Application.streamingAssetsPath + "/" + BookName; ;//弄一个文件夹，用来存放一千多个章节的txt
            string ZhangJieUrl = BookUrl + "/第" + (ShowZhangjie_num + 1) + "章.txt";//给每一章一个名字吧，方便加载
            if (!File.Exists(ZhangJieUrl))
            {//把分别把每一章写进去创建成txt
                FileStream fs1 = new FileStream(ZhangJieUrl, FileMode.Create, FileAccess.Write);//创建写入文件 
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine(finalstr);//开始写入值
                sw.Close();
                sw.Dispose();
                fs1.Close();
                fs1.Dispose();
                //注意close和dispose，不释放unity会卡死
            }

            Zhangjie_num++;
            ShowZhangjie_num++;
            yield return 1;
            //写完一章debug一下，让你不至于那么焦躁，最终写完了告诉你allend，此处的582，是拿最后一章的index减去第一章的index算出来的，应该还要+1，不然会缺失最后一章
            Debug.Log("Write_end" + Zhangjie_num);
            if (Zhangjie_num < 582)
            {
                StartCoroutine(GetHttpInfo());
            }
            else
            {
                Debug.Log("Allend!!!!!!!!!");
            }
        }
    }



}
