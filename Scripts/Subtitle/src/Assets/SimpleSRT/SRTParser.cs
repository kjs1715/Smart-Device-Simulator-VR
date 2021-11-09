using System;
using System.Collections.Generic;
using UnityEngine;

public class SRTParser
{
  enum eReadState
  {
    Index,
    Time,
    Text
  }
    // 解析后的字幕
  List<SubtitleBlock> _subtitles;

  public int subPointer = 0;

  public SRTParser(string textAssetResourcePath)
  {
    var text = Resources.Load<TextAsset>(textAssetResourcePath);
    Load(text);
  }

  public SRTParser(TextAsset textAsset)
  {
    this._subtitles = Load(textAsset);
    int count = 0;
    while (count < subPointer) {
      _subtitles.RemoveAt(0);
      count++;
      Debug.Log("Removed?");
    }
  }

    // 加载srt文件
  static public List<SubtitleBlock> Load(TextAsset textAsset)
  {
    if (textAsset == null)
    {
      Debug.LogError("Subtitle file is null");
      return null;
    }

    // 分成行
    var lines = textAsset.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

    // 当前读取状态
    // 要读取Index
    var currentState = eReadState.Index;

    var subs = new List<SubtitleBlock>();

    int currentIndex = 0;
    double currentFrom = 0, currentTo = 0;
    var currentText = string.Empty;

    // 遍历行
    for (var l = 0; l < lines.Length; l++)
    {
      var line = lines[l];

      switch (currentState)
      {
        case eReadState.Index:
          {
            int index;
            if (Int32.TryParse(line, out index))
            {
              currentIndex = index;

              // 当前读取状态
              // 需要读取时间
              currentState = eReadState.Time;
            }
          }
          break;

        case eReadState.Time:
          {
            line = line.Replace(',', '.');

            // 分开，
            // 开始时间
            // 结束时间
            var parts = line.Split(new[] { "-->" }, StringSplitOptions.RemoveEmptyEntries);

            // Parse the timestamps
            if (parts.Length == 2)
            {
              TimeSpan fromTime;
              if (TimeSpan.TryParse(parts[0], out fromTime))
              {
                TimeSpan toTime;
                if (TimeSpan.TryParse(parts[1], out toTime))
                {
                    // 时间，保存成单位s
                  currentFrom = fromTime.TotalSeconds;
                  currentTo = toTime.TotalSeconds;

                  // 当前读取状态
                  // 字幕文本
                  currentState = eReadState.Text;
                }
              }
            }
          }
          break;

        case eReadState.Text:
          {
            // 需要空行
            // 标记文本结束
			if (currentText != string.Empty)
				currentText += "\r\n";

            currentText += line;

            // 读取结束
            // 创建SubtitleBlock
            // When we hit an empty line, consider it the end of the text
            if (string.IsNullOrEmpty(line) || l == lines.Length - 1)
            {
              // Create the SubtitleBlock with the data we've aquired 
              subs.Add(new SubtitleBlock(currentIndex, currentFrom, currentTo, currentText));

              // Reset stuff so we can start again for the next block
              currentText = string.Empty;

              // 当前读取状态
              // index
              currentState = eReadState.Index;
            }
          }
          break;
      }
    }
	return subs;
  }

  public SubtitleBlock GetForTime(float time)
  {
    if (_subtitles.Count > 0)
    {
        // subtitle: 指向第一个字幕
      var subtitle = _subtitles[subPointer];

        // 如果时间已经超过了此字幕
        // 移除
      if (time >= subtitle.To)
      {
        // _subtitles.RemoveAt(0);
        subPointer += 1;

        if (_subtitles.Count == 0)
          return null;

        // 更新subtitle
        // 仍指向第一个字幕
        subtitle = _subtitles[subPointer];
      }

        // Q: ?
      if (subtitle.From > time)
        return SubtitleBlock.Blank;

      // Debug.Log(subPointer);
      // Debug.Log(subtitle.Text);
      return subtitle;
    }
    return null;
  }
}

// 字幕的一段
public class SubtitleBlock
{
  static SubtitleBlock _blank;
  public static SubtitleBlock Blank
  {
    get { return _blank ?? (_blank = new SubtitleBlock(0, 0, 0, string.Empty)); }
  }

  public int Index { get; private set; }
  public double Length { get; private set; }
  public double From { get; private set; }
  public double To { get; private set; }
  public string Text { get; private set; }

  public SubtitleBlock(int index, double from, double to, string text)
  {
    this.Index = index;
    this.From = from;
    this.To = to;
    this.Length = to - from;
    this.Text = text;
  }
}
