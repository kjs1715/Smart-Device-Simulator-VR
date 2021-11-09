using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Solution {

    public List<SolutionModel> list;
    public double totalScore;
    public Solution() {
        list = new List<SolutionModel>(); 
    }

    public override string ToString()
    {   
        if (list.Count > 0) {
            string str = "";
            foreach (SolutionModel model in list) {
                // str += model.
                str += "D" + model._device + " - C" + model._content + " M" + model._method + '\n';
            }
            str += "Total Score: " + totalScore;
            return str;
        }
        return "NO POLICY";
    }

}