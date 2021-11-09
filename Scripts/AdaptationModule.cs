using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class AdaptationModule : MonoBehaviour
{


    public DeviceDetector deviceDetector;
    public ContextManager contextManager;

    // Consts
    const int CO_COUNTS = 36;
    int CHANNEL_LIMIT = 0;
    int METHOD_COUNTS;
    int CONTENT_COUNTS;



    // Coefficients
    double[] a, b, c;
    double[] gt_dis2size, gt_visibility, gt_angle;


    // last contexts
    int lastDeviceNum = -1;


    // channel combination
    bool channel_visual = false;
    bool channel_visual_part = false;
    bool channel_auditory = false;


    // last policy
    Solution lastSolution;


    // policy mode
    int BEST_POLICY = 0;
    int SECOND_POLICY = 0;

    public bool isBestPolicy = true;


    

    void Start()
    {
        // set coefficients
        a = new double[CO_COUNTS];
        b = new double[CO_COUNTS];
        c = new double[CO_COUNTS];

        gt_dis2size = new double[CO_COUNTS];
        gt_visibility = new double[CO_COUNTS];
        gt_angle = new double[CO_COUNTS];

        // set limit counts
        CHANNEL_LIMIT = CO_COUNTS / 4;



        lastSolution = new Solution();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartAdaptation(bool[] channel) {
        if (!channel[0] && !channel[1] && !channel[2] && !channel[3]) {

            Debug.Log("No need to adaptation...");
            return ;
        }
        Debug.Log("Start Adaptation");
        // need constraints (more to 3 devices, each content less than 1)
        
        // Calculate scores (record max and min)
        double MAX_SCORE = -1000.0f;
        double MIN_SCORE = 1000.0f;

        List<SolutionModel> score_list = new List<SolutionModel>();
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                for (int k = 0; k < 4; k++) {
                    if (!channel[k]) {
                        Debug.Log("Channel " + k + " no limit");
                        continue;
                    }
            
                    for (int m = 0; m < deviceDetector.getDeviceCount(); m++) {
                        float size = deviceDetector.devices[m].device_actual_size;
                        float distance = contextManager.user2deviceDistance[m];
                        float visibility = deviceDetector.devices[m].Visibility;
                        float angle = contextManager.user2deviceAngle[m];
                        SolutionModel model = Formula(size: size, distance: distance, visibility: visibility, angle: angle, content: i, method: j, channel: k, device: m);
                        score_list.Add(model);

                        // max min
                        if (model.GetScore() > MAX_SCORE) {
                            MAX_SCORE = model.GetScore();
                        }
                        if (model.GetScore() < MIN_SCORE) {
                            MIN_SCORE = model.GetScore();
                        }
                    }
                }
            }
        }

        // Debug.Log("MAX :" + MAX_SCORE);
        // Debug.Log("MAX :" + MIN_SCORE);

        // score normalize
        foreach (SolutionModel m in score_list) {
            m.SetScore((6 / MAX_SCORE - MIN_SCORE) * (m.GetScore() - MIN_SCORE) + 1);    // TODO: stiil need to consider?
            // Debug.Log(m.ToString());
        }
        Debug.Log(score_list.Count);

        // set content and method start point

        int video_content_start = 0;
        int audio_content_start = score_list.Count / 3;
        int sub_content_start = 2 * score_list.Count / 3;

        CONTENT_COUNTS = score_list.Count / 3;
        METHOD_COUNTS = CONTENT_COUNTS / 3;


        // max pair in each content-method-pair
        List<SolutionModel> contentMethodList = new List<SolutionModel> ();
        for (int i = 0; i < 3; i++) {
            contentMethodList.Add(GetMAXFromEachContentMethod(video_content_start, i, score_list));
            contentMethodList.Add(GetMAXFromEachContentMethod(audio_content_start, i, score_list));
            contentMethodList.Add(GetMAXFromEachContentMethod(sub_content_start, i, score_list));
        }

        // combinations
        int count = 0;
        List<Solution> solList = new List<Solution>();
        IEnumerable<SolutionModel[]> combList = Combinations<SolutionModel>(contentMethodList);
        foreach (SolutionModel[] models in combList) {
            string str = "";
            Solution solution = new Solution();
            foreach (SolutionModel model in models) {
                solution.list.Add(model);
                solution.totalScore += model.GetScore();
            }
            // Debug.Log(str);
            count += 1;
            solList.Add(solution);
        }

        // Check duplicate device-contents pair
        List<Solution> new_solList = CheckDuplicate(solList);

        // check duplicate solution
        List<Solution> sol_dupList = new List<Solution>();
        foreach (Solution s in new_solList) {
            // if (!sol_dupList.Contains(s)) {
            //     sol_dupList.Add(s);
            // }

            Dictionary<int, List<SolutionModel>> deviceDict = new Dictionary<int, List<SolutionModel>>();
            foreach (SolutionModel model in s.list) {
                if (!deviceDict.ContainsKey(model._device)) {
                    List<SolutionModel> sl = new List<SolutionModel>();
                    sl.Add(model);
                    deviceDict.Add(model._device, sl);
                } else {
                    deviceDict[model._device].Add(model);
                }

            }
            // device cost
            if (deviceDict.Count > 3) {
                Debug.Log("Minus cost ");
                Solution new_s = s;
                new_s.totalScore -= DeviceCountCost(deviceDict.Count);
                sol_dupList.Add(new_s);
            } else {
                sol_dupList.Add(s);
            }
        }




        // sort solution
        new_solList = sol_dupList.OrderByDescending(i => i.totalScore).ToList();


        // test output
        Debug.Log("Solutions:");
        for (int i = 0; i < 5; i++) {
            Debug.Log(new_solList[i].ToString());
        }

        SECOND_POLICY = new_solList.Count/2;

        Debug.Log("Best policy: " + new_solList[0]);
        Debug.Log("Second policy: " + new_solList[SECOND_POLICY]);

        // TODO: test duplicate part
        // List<Solution> test_list = new List<Solution>();
        // for (int i = 0; i < 2; i++) {
        //     test_list.Add(new_solList[i]);
        // }


        // return solution
        Solution sol = new Solution();
        if (isBestPolicy) {
            sol = new_solList[BEST_POLICY];
            Debug.Log("Using best policy");
        } else {
            sol = new_solList[SECOND_POLICY];
            Debug.Log("Using second policy");
        }
        if (contextManager.currentTask != null) {
            if (lastSolution != sol) {
                contextManager.Reschedule(sol);
                Debug.Log("Rescheduling");
                lastSolution = sol;
            }
            Debug.Log("Not Rescheduling");
        } else {
            Debug.Log("Not Rescheduling");

        }
    }

    public SolutionModel Formula(float size, float distance, float visibility, float angle, int content, int method, int channel, int device) {
        int index = content * 6 + method * 3 + channel;

        // x1
        double x1 = -Mathf.Abs( (Mathf.Pow( distance, 2 ) / size / 10) - (float) gt_dis2size[index] );
        
        // x2
        double x2 = -Mathf.Exp( -Mathf.Pow( (visibility - (float) gt_visibility[index]), 2) );

        // x3
        double x3 = -Mathf.Exp( -Mathf.Sin( Mathf.PI / 180 * (angle - (float) gt_angle[index]) / 2 ) + 1);


        double score = a[index] * x1 + b[index] * x2 + c[index] * x3;
        // Debug.Log("Content, Method, Device : (" + content + ", " + method  + ", " + device + ")\nx1, x2, x3 ( :" + x1 + ", " + x2 + "," + x3 + ")" + '\n' + "a * x1, b *x2, c * x3 : (" + a[index] * x1 + " " + b[index] * x2 + " " + c[index] * x3 + ")\n" + score);

        return new SolutionModel(content, method, device, score);
    }

    public SolutionModel GetMAXFromEachContentMethod(int content_start, int method_start, List<SolutionModel> scorelist) {
        // Debug.Log(content_start + " " + method_start);

        SolutionModel MAX_model = new SolutionModel(-1, -1, -1 , -1000);
        int content_method_start_point = content_start + method_start * METHOD_COUNTS;
        // Debug.Log(content_method_start_point);

        for (int i = content_method_start_point; i < content_method_start_point + METHOD_COUNTS; i++) {
            SolutionModel temp = (SolutionModel) scorelist[i];
            // Debug.Log(content_start + " " + method_start + " " + temp);
            if (MAX_model.GetScore() < temp.GetScore()) {
                MAX_model = temp;
            }
        }
        return MAX_model;
    }

    public static IEnumerable<T[]> Combinations<T> (IEnumerable<T> source) {
        // if (null == source) {
        //     throw new ArgumentNullExce
        // }
        T[] data = source.ToArray();
        return Enumerable
            .Range(0, 1 << (data.Length))
            .Select(index => data
                .Where((v, i) => (index & (1 << i)) != 0)
                .ToArray());
    }

    public List<Solution> CheckDuplicate(List<Solution> solutions) {
        List<Solution> new_solutions = new List<Solution>();
        
        foreach (Solution solution in solutions) {
            // count device-content pair
            Dictionary<string, int> deviceDict = new Dictionary<string, int>();
            foreach (SolutionModel model in solution.list) {
                string key = model._device.ToString() + " " + model._content.ToString();
                if (!deviceDict.ContainsKey(key)) {
                    deviceDict.Add(key, 1);
                } else {
                    deviceDict[key] += 1;
                }
            }

            // iter all duplicated dictionary
            Solution new_solution = CompareModelScores(solution, deviceDict);

            new_solutions.Add(new_solution);
        }

        return new_solutions;
    }

    public Solution CompareModelScores(Solution solution, Dictionary<string, int> dict) {
        // key -> pair

        List<int> deleteList = new List<int>();

        foreach (KeyValuePair<string, int> pair in dict) {
            if (pair.Value <= 1) {
                // skip other pairs
                continue ;
            }
            string[] device_content = pair.Key.Split(' ');
            int deviceIndex = int.Parse(device_content[0]);
            int contentIndex = int.Parse(device_content[1]);
            
            // check index num and delete dupliated policy later
            double HighestScore = -10000.0f;
            int HighestIndex = -1;
            for (int i = 0; i < solution.list.Count; i++) {
                SolutionModel model = solution.list[i];
                if (model._device == deviceIndex && model._content == contentIndex) {
                    if (model.GetScore() > HighestScore) {
                        // check max index
                        if (HighestIndex != -1) {
                            // first highest num should be deleted
                            deleteList.Add(HighestIndex);
                        }
                        HighestScore = model.GetScore();
                        HighestIndex = i;
                    } else {
                        deleteList.Add(i);
                    }
                }
            }
        }

        Solution new_solution = new Solution();
        if (deleteList.Count > 0) {
            // Debug.Log("Need Deletion..." + deleteList.Count);
            // copy a new solution
            for (int i = 0; i < solution.list.Count; i++) {
                if (!deleteList.Contains(i)) {
                    new_solution.list.Add(solution.list[i]);
                    new_solution.totalScore += solution.list[i].GetScore();
                }
            }
        } else {
            new_solution = solution;
        }


        return new_solution;

    }   

    public double DeviceCountCost(int deviceCount) {
        if (deviceCount <= 2)
            return 0.0f;
        return deviceCount * 500.0f;
    }



    public void SetCoefficient(string str) {
        string[] temp = str.Split(',');
        string[] Coeffients = temp[0].Split(' ');
        string[] Groundtruth = temp[1].Split(' ');
        Debug.Log(Coeffients.ToString());
        Debug.Log(Groundtruth.ToString());

        for (int i = 0; i < CO_COUNTS; i++) {
            a[i] = double.Parse(Coeffients[i * 3]);
            b[i] = double.Parse(Coeffients[i * 3 + 1]);
            c[i] = double.Parse(Coeffients[i * 3 + 2]);

            gt_dis2size[i] = double.Parse(Groundtruth[i * 3]);
            gt_visibility[i] = double.Parse(Groundtruth[i * 3 + 1]);
            gt_angle[i] = double.Parse(Groundtruth[i * 3 + 2]);


        }
        Debug.Log("Set coefficients...");
        Debug.Log(this.ToString());
    }

    public override string ToString()
    {
        string str = "";

        str += "Coefficients\n";
        for (int i = 0; i < CO_COUNTS; i++) {
            str += "index :" + i + "    " + a[i] + "     " + b[i] + "     " + c[i] + "\n";
        }

        str += "Ground Truth";
        for (int i = 0; i < CO_COUNTS; i++) {
            str += "index :" + i + "    " + gt_dis2size[i] + "     " + gt_visibility[i] + "     " + gt_angle[i] + "\n";
        }
        return str;
    }
}
