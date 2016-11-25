// PclWrapper.h

#pragma once

#include <string>
#include <iostream>
#include <pcl/io/pcd_io.h>
#include <pcl/point_representation.h>
#include <pcl/point_types.h>
#include <pcl/registration/icp.h>

using namespace System;

namespace PclWrapper {

	public value struct Points {
	public:
		float x;
		float y;
		float z;
		float d;
		uint8_t b;
		uint8_t g;
		uint8_t r;
		uint8_t a;
		float d1;
		float d2;
		float d3;
	};

	//void RegularizeDistribution(pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloudIn, pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloudOut);
	//void calculateICP(pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud1, pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud2, float* t);

	public ref class PCD
	{
	public:
		array<Points>^ LoadPointcloud(String^ Name);
		//void LoadPointcloud(String^ Name, array<Points>^ PointList);
		void SavePointcloud(char* Name, array<Points>^ cloud);

		/// Userdefined processing methods
		array<Points>^ Process1(array<Points>^ cloud);
		array<Points>^ Process2(array<Points>^ cloud);
		array<Points>^ Process3(array<Points>^ cloud);

		//array<Points>^ Process4(array<Points>^ cloud1, array<Points>^ cloud2);
		void Process4(array<Points>^ cloud1, array<Points>^ cloud2, array<float>^ transformation);

	private:
		array<Points>^ convertPointcloudToPoints(pcl::PointCloud<pcl::PointXYZRGB>::Ptr p);
		pcl::PointCloud<pcl::PointXYZRGB>::Ptr convertPointsToPointcloud(array<Points>^ p);
	};
}
