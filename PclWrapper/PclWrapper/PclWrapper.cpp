// This is the main DLL file.

#include "stdafx.h"
#include <string>
#include <iostream>
#include "PclWrapper.h"

array<PclWrapper::Points>^ PclWrapper::PCD::LoadPointcloud(String^ Name)
{
	//Convert string type
	IntPtr ptr = System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Name);
	char* str = static_cast<char*>(ptr.ToPointer());

	//Initialize point cloud and read it from file
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZRGB>);

	if (pcl::io::loadPCDFile<pcl::PointXYZRGB>(str, *cloud) == -1) //* load the file
	{
		PCL_ERROR("Couldn't read file test_pcd.pcd \n");
	}

	return convertPointcloudToPoints(cloud);
}

//void PclWrapper::PCD::LoadPointcloud(String^ Name, array<PclWrapper::Points>^ PointList)
//{
//	//Convert string type
//	IntPtr ptr = System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(Name);
//	char* str = static_cast<char*>(ptr.ToPointer());
//
//	//Initialize point cloud and read it from file
//	pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZRGB>);
//
//	if (pcl::io::loadPCDFile<pcl::PointXYZRGB>(str, *cloud) == -1) //* load the file
//	{
//		PCL_ERROR("Couldn't read file test_pcd.pcd \n");
//		return;
//	}
//
//	//return convertPointcloudToPoints(cloud);
//}

void PclWrapper::PCD::SavePointcloud(char* Name, array<Points>^ cloud)
{
	pcl::io::savePCDFileASCII(Name, *convertPointsToPointcloud(cloud));
}

array<PclWrapper::Points>^ PclWrapper::PCD::convertPointcloudToPoints(pcl::PointCloud<pcl::PointXYZRGB>::Ptr p)
{
	array<PclWrapper::Points>^ pointcloud = gcnew array<PclWrapper::Points>(p->points.size());

	for (int i = 0; i < pointcloud->Length; ++i)
	{
		pointcloud[i].x = p->points[i].x;
		pointcloud[i].y = p->points[i].y;
		pointcloud[i].z = p->points[i].z;
		pointcloud[i].r = p->points[i].r;
		pointcloud[i].g = p->points[i].g;
		pointcloud[i].b = p->points[i].b;
	}

	return pointcloud;
}

pcl::PointCloud<pcl::PointXYZRGB>::Ptr PclWrapper::PCD::convertPointsToPointcloud(array<PclWrapper::Points>^ p)
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr NewPointCloud(new pcl::PointCloud<pcl::PointXYZRGB>);

	NewPointCloud->width = p->Length;
	NewPointCloud->height = 1;
	NewPointCloud->is_dense = false;
	NewPointCloud->points.resize(NewPointCloud->width * NewPointCloud->height);

	for (int i = 0; i < p->Length; i++)
	{
		NewPointCloud->points[i].x = p[i].x;
		NewPointCloud->points[i].y = p[i].y;
		NewPointCloud->points[i].z = p[i].z;
		NewPointCloud->points[i].r = p[i].r;
		NewPointCloud->points[i].g = p[i].g;
		NewPointCloud->points[i].b = p[i].b;
	}

	return NewPointCloud;
}

array<PclWrapper::Points>^ PclWrapper::PCD::Process1(array<Points>^ cloud)
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud = convertPointsToPointcloud(cloud);

	// Do your processing here

	return convertPointcloudToPoints(pointcloud);
}

array<PclWrapper::Points>^ PclWrapper::PCD::Process2(array<Points>^ cloud)
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud = convertPointsToPointcloud(cloud);

	// Do your processing here

	return convertPointcloudToPoints(pointcloud);
}

array<PclWrapper::Points>^ PclWrapper::PCD::Process3(array<Points>^ cloud)
{
	pcl::PointCloud<pcl::PointXYZRGB>::Ptr pointcloud = convertPointsToPointcloud(cloud);

	// Do your processing here

	return convertPointcloudToPoints(pointcloud);
}
