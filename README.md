# unity_AR_to_model_app

Introduction
============
This is a class project of Augmented Reality class in Johns Hopkins University.

This app is written with Unity3D and supposed to be run on Meta 2. The purpose of this project is to find out the pose of a real object in front of user and visualize a virtual object on top of it. Ideally, the virtual object should be at the exact same position as the real object. The 3D model of the object to be found is known.

Specifically, the pose is found by performing registration between the 3d point cloud data obtained by sensors on Meta 2 and the known 3d model. This is why we are using Meta 2 as HoloLens cannot give us 3d point cloud data currently.

Point Cloud Library (PCL) is used in this app to help facilitate the development. As PCL is written in C++, a wrapper is written to compile those useful function in PCL into dll so that it could be used in Unity with C#. (See my other project "pcl_unity_wrapper")