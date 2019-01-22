using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using Meta;
using UnityEngine.UI;
[RequireComponent(typeof(LineRenderer))]

//using pcl;

public class testPC : MetaBehaviour
{
    public Text countDist;
    [DllImport("wrapper", EntryPoint = "detectplane")]
    public unsafe static extern int detectplane(float* points, int size, float* transInfo);
    [DllImport("wrapper", EntryPoint = "dataConverter")]
    public unsafe static extern float[] dataConverter(float* points, int size, float* initial_guess, bool isFirst);

    bool isFirst = true;
    bool pause = true;
    bool pause2 = true;
    int cnt = 0;
    float[] real_obj = new float[8] {0,0,0,1,0,0,0,0};
    //LineRenderer line;
    float pre_score = 0;
    Vector3 prev_frame_trans = new Vector3(10,10,10);
    Quaternion pre_frame_rot = new Quaternion(1, 0, 0, 0);
    int updateRate = 25;
    void Update()
    {
        GameObject myObject = GameObject.Find("object");
        GameObject myCam = GameObject.Find("myCamera");
        GameObject my_real_object = GameObject.Find("realObject");
        GameObject Line = GameObject.Find("Line");
        GameObject arrow = GameObject.Find("TransArrow");
        GameObject rotLine1 = GameObject.Find("rotLine1");
        GameObject rotLine2 = GameObject.Find("rotLine2");
        GameObject text = GameObject.Find("text");
        GameObject success = GameObject.Find("success");
        //var line = my_real_object.AddComponent<LineRenderer>();
        //Transform my_real_object;

        if (pause == false)
        {
            //Debug.Log("start the app");
            Time.timeScale = 1;

            //Debug.Log("start the main function");
            //Time.timeScale = 1;
            
            cnt = cnt + 1;
            
            if (cnt % updateRate == 0)
            {
                //Debug.Log("enter the pointclouddata");
                PointCloudMetaData metadata = new PointCloudMetaData();
                metaContext.Get<InteractionEngine>().GetCloudMetaData(ref metadata);
                PointCloudData<PointXYZConfidence> pointCloudData = new PointCloudData<PointXYZConfidence>(metadata.maxSize);
                metaContext.Get<InteractionEngine>().GetCloudData(ref pointCloudData);
                //Debug.Log(pointCloudData.size);
                //Use point cloud data here

                /*
                for (int i = 0; i < pointCloudData.size; i++)
                {
                    string pointinfo = pointCloudData.points[i].ToString();
                    Debug.Log(i);
                    System.IO.File.WriteAllText(path: @"D:\test1.txt", contents: pointinfo);
                }*/
                /*
                var type = pointCloudData.points[0].vertex.GetType();
                Debug.Log(type);
                var sizetype = pointCloudData.points[0].vertex[0].GetType();
                Debug.Log("xxxxxxx"+sizetype);
                */
                int length = pointCloudData.points.Length;
                int size = pointCloudData.size;
                //Debug.Log("point_size=" + size);
                //Debug.Log("point_length=" + length);

                float[] psource = new float[size * 3];
                float[] transInfo = new float[7];
                unsafe
                {
                    fixed (float* temp_p = psource)
                    {
                        fixed (float* transinfo = transInfo)
                        {
                            float* pt = temp_p;
                            int num = 0;
                            foreach (PointXYZConfidence point in pointCloudData.points)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    if (i == 0)
                                    {
                                        *pt = point.vertex[i];
                                    }
                                    else if (i == 1)
                                    {
                                        *pt = -point.vertex[i];
                                    }
                                    else
                                    {
                                        *pt = point.vertex[i];
                                    }
                                    pt++;
                                    num++;
                                }
                                if (num >= size * 3) break;
                            }
                            //Debug.Log("num:" + num);
                            pt = null;
                            
                            if (pause2 == false)
                            {
                                // get cuurent pose of real object
                                float[] temp_real = new float[8];
                                fixed (float* initial_guess_= real_obj)
                                {
                                    temp_real = dataConverter(temp_p, num, initial_guess_, isFirst);
                                }
                                if (temp_real[0] == -1) return;

                                // store the result in C# format
                                Quaternion rot_real = new Quaternion(temp_real[1], temp_real[2], temp_real[3], temp_real[0]);
                                Vector3 trans_real = new Vector3(temp_real[4], temp_real[5], temp_real[6]);
                                float score = temp_real[7];
                                
                                // get last valid pose
                                Vector3 real_obj_trans = new Vector3(real_obj[4], real_obj[5], real_obj[6]);
                                Quaternion real_obj_rot = new Quaternion(real_obj[1], real_obj[2], real_obj[3], real_obj[0]);
                                
                                // translational distance between current and last pose estimation
                                float frame_dist = (prev_frame_trans - trans_real).magnitude;
                                bool inFOV = trans_real.z > 0.12 && trans_real.x <  trans_real.z && trans_real.x > - trans_real.z && trans_real.y <  0.6*trans_real.z && trans_real.y > -0.6 * trans_real.z;
                                
                                // continue if the icp position is at good location
                                if (score < 0.001 && frame_dist < 0.1 && inFOV) // 
                                {
                                    if (isFirst)
                                    {
                                        for (int i=0; i < 8; i++)
                                        {
                                            real_obj[i] = temp_real[i];
                                        }
                                    }

                                    updateRate = 10;
                                    // whether curent pose estimation is close to last valid pose
                                    bool close_to_last = (trans_real-real_obj_trans).magnitude < 0.1 && (Quaternion.Dot(real_obj_rot,rot_real)>0.65);//====================
                                    
                                    if (close_to_last || isFirst)
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            real_obj[i] = temp_real[i];
                                        }
                                        // ================== update real object location and draw lines =====================
                                        my_real_object.transform.rotation = myCam.transform.rotation;
                                        my_real_object.transform.position = myCam.transform.position;

                                        my_real_object.transform.Translate(trans_real, Space.Self);
                                        my_real_object.transform.Rotate(rot_real.eulerAngles, Space.Self);

                                        var line = Line.GetComponent<LineRenderer>();
                                        line.sortingOrder = 1;
                                        var rotline1 = rotLine1.GetComponent<LineRenderer>();
                                        rotline1.sortingOrder = 1;
                                        var rotline2 = rotLine2.GetComponent<LineRenderer>();
                                        rotline2.sortingOrder = 1;
                                        var myArrow = arrow.GetComponent<LineRenderer>();
                                        myArrow.sortingOrder = 1;

                                        Quaternion offset1 = new Quaternion(0, 0, 1, 0);
                                        Quaternion offset2 = new Quaternion(0, 1, 0, 0);

                                        Quaternion rot_diff = Quaternion.Inverse(my_real_object.transform.rotation * offset1) * myObject.transform.rotation * offset2;
                                        Vector3 rot_axis = new Vector3(0, 0, 0);
                                        float rot_angle = 0;
                                        rot_diff.ToAngleAxis(out rot_angle, out rot_axis);

                                        // -------------draw translational distance-------------------

                                        var point1 = my_real_object.transform.position;
                                        var point2 = myObject.transform.position;
                                        point1.y += 0.05f;
                                        point2.y += 0.05f;
                                        
                                        Vector3 point3 = Vector3.Lerp(point1, point2, 0.7f);
                                        Vector3 point4 = Vector3.Lerp(point1, point2, 0.9f);
                                        line.positionCount = 2;
                                        myArrow.positionCount = 2;
                                        // set color accroding to distance
                                        var distance_real_to_model = (point1 - point2).magnitude;
                                        if (distance_real_to_model > 0.15)
                                        {
                                            line.material.color = Color.red;
                                            myArrow.material.color = Color.red;
                                        }
                                        else if (distance_real_to_model > 0.1)
                                        {
                                            line.material.color = Color.yellow;
                                            myArrow.material.color = Color.yellow;
                                        }
                                        else
                                        {
                                            line.material.color = Color.green;
                                            myArrow.material.color = Color.green;
                                        }

                                        line.SetPosition(0, point1);
                                        line.SetPosition(1, point3);
                                        line.SetWidth(0.0125f, 0.0125f);
                                        line.useWorldSpace = true;
                                        myArrow.SetPosition(0, point3);
                                        myArrow.SetPosition(1, point4);
                                        myArrow.SetWidth(0.025f, 0.0f);
                                        line.useWorldSpace = true;

                                        // -------------------draw rotational distance -----------------------
                                        var point1r = point1;
                                        point1r = point1r- 0.2f * my_real_object.transform.right;
                                        var point2r = point1;
                                        point2r = point2r - 0.2f * myObject.transform.right;

                                        var visualCo = 2.0f;
                                        point2r.x += (point2.x - point1.x);// / (point2 - point1).magnitude;
                                        point2r.y += (point2.y - point1.y);// / (point2 - point1).magnitude;
                                        point2r.z += (point2.z - point1.z);// / (point2 - point1).magnitude;

                                        rotline1.positionCount = 2;
                                        rotline1.SetPosition(0, point1);
                                        rotline1.SetPosition(1, point1r);
                                        rotline1.SetWidth(0.005f, 0.005f);
                                        rotline1.useWorldSpace = true;
                                        rotline1.material.color = Color.red;

                                        rotline2.positionCount = 2;
                                        rotline2.SetPosition(0, point1);
                                        rotline2.SetPosition(1, point2r);
                                        rotline2.SetWidth(0.005f, 0.005f);
                                        rotline2.useWorldSpace = true;
                                        rotline2.material.color = Color.yellow;
                                        // ----------------visualize the distance error---------------------------
                                        //var distance_display = (point1 - point2).magnitude;
                                        countDist.text = "Matching Complete";
                                        //TextMesh t = text.AddComponent<TextMesh>();
                                        //
                                        //t.text = 
                                        //t.fontSize = 30;
                                        //t.transform.localEulerAngles += new Vector3(90, 0, 0);
                                        //var point2text = point2;
                                        //point2text.y += 0.1f;
                                        countDist.transform.position = myObject.transform.position;

                                        //==============================visualization end =======================================
                                        if (distance_real_to_model < 0.03 && (Quaternion.Dot(real_obj_rot, rot_real) > 0.85 ) )
                                        {
                                            success.transform.position = myObject.transform.position - myObject.transform.right * 0.05f + myObject.transform.up * 0.15f;
                                        }
                                    }
                                    isFirst = false;
                                }
                                
                                prev_frame_trans = trans_real;
                                pre_frame_rot = rot_real;
                                pre_score = score;
                            }
                            else
                            { 
                                detectplane(temp_p, num, transinfo);
                                Vector3 trans_cam2obj = new Vector3(transinfo[0], transinfo[1], transinfo[2]);
                                Quaternion rot_cam2obj = new Quaternion(transinfo[3], transinfo[4], transinfo[5], transinfo[6]);
                                
                               
                                //Vector3 trans_world2cam = myCam.transform.position;
                                //Quaternion rot_world2cam = myCam.transform.rotation;// * tempQ;
                                myObject.transform.rotation = myCam.transform.rotation;
                                myObject.transform.position = myCam.transform.position;
                                

                                myObject.transform.Translate(trans_cam2obj, Space.Self);
                                myObject.transform.Rotate(rot_cam2obj.eulerAngles, Space.Self);
                                
                                

                                //-------------------------------------------
                                //myObject.transform.rotation = rot_world2cam;
                                //myObject.transform.position = trans_world2cam;


                                //Quaternion new_rot_cam2obj = Quaternion.Inverse(rot_cam2obj);

                                //myObject.transform.localPosition = trans_cam2obj;
                                //myObject.transform.localRotation = rot_cam2obj;
                                //myObject.transform.rotation = rot_world2cam*rot_cam2obj;
                                //myObject.transform.position = myCam.transform.TransformPoint(trans_cam2obj);
                                //Debug.Log("xyz"+myObject.transform.position.x + myObject.transform.position.y + myObject.transform.position.z);

                            }



                        }
                    }

                }
            }
        }

        else
        {
            Time.timeScale = 1;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (pause == true)
            {
                pause = false;
            }
            else
            {
                pause = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (pause == true)
            {
                pause = true;
            }
            else
            {
                pause = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (pause2 == true)
            {
                pause2 = false;
            }
            else
            {
                pause2 = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (pause2 == true)
            {
                pause2 = true;
            }
            else
            {
                pause2 = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isFirst = true;
        }
    }
}