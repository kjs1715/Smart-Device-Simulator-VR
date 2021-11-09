using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SolutionModel {
    public int _content;
    public int _method;
    public int _device;
    double _score;

    public SolutionModel(int c, int m, int d, double s) {
        _content = c;
        _method = m;
        _device = d;
        _score = s;
    }

    public void SetScore(double value) {
        _score = value;
    }

    public double GetScore() {
        return _score;
    }

    public override string ToString() {
        string str = "";
        str += "Content :" + _content + " Method :" + _method + " Device :" + _device + "Score = " + _score;
        return str;
    }

}